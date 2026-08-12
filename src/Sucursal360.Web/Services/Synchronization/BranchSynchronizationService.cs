using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Integrations.Abstractions;

namespace Sucursal360.Web.Services.Synchronization;

public sealed class BranchSynchronizationService(
    ApplicationDbContext dbContext,
    IPublicBranchDataProvider publicBranchDataProvider,
    ILogger<BranchSynchronizationService> logger) : IBranchSynchronizationService
{
    public async Task<SynchronizationResult> SynchronizeBranchAsync(
        Guid branchId,
        string triggeredByUserId,
        CancellationToken cancellationToken)
    {
        var branch = await dbContext.Branches.SingleOrDefaultAsync(candidate => candidate.Id == branchId, cancellationToken);
        if (branch is null)
        {
            return new SynchronizationResult(
                null,
                "No disponible",
                "No disponible",
                Guid.Empty,
                string.Empty,
                IntegrationRunStatus.Failed,
                0,
                0,
                "La sucursal solicitada no existe.",
                "APP-404-BRANCH");
        }

        if (!branch.IsActive || string.IsNullOrWhiteSpace(branch.ExternalPlaceId))
        {
            return await CreateRejectedRunAsync(
                branch,
                triggeredByUserId,
                "INT-400-CONFIG",
                "Revise la configuracion de la sucursal.",
                "Branch is inactive or missing external id.",
                cancellationToken);
        }

        if (branch.Provider != publicBranchDataProvider.Provider)
        {
            return await CreateRejectedRunAsync(
                branch,
                triggeredByUserId,
                "INT-400-PROVIDER",
                "El proveedor configurado no esta habilitado.",
                $"Configured provider {branch.Provider} does not match active provider {publicBranchDataProvider.Provider}.",
                cancellationToken);
        }

        var hasRunInProgress = await dbContext.IntegrationRuns.AnyAsync(
            run => run.BranchId == branch.Id && run.Status == IntegrationRunStatus.InProgress,
            cancellationToken);
        if (hasRunInProgress)
        {
            return await CreateRejectedRunAsync(
                branch,
                triggeredByUserId,
                "INT-409-RUNNING",
                "Ya existe una sincronizacion en curso.",
                "An integration run is already in progress for this branch.",
                cancellationToken);
        }

        var run = new IntegrationRun
        {
            Id = Guid.NewGuid(),
            CorrelationId = CreateCorrelationId(),
            Provider = branch.Provider,
            BranchId = branch.Id,
            StartedAtUtc = DateTimeOffset.UtcNow,
            Status = IntegrationRunStatus.InProgress,
            TriggeredByUserId = triggeredByUserId
        };

        dbContext.IntegrationRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var externalData = await publicBranchDataProvider.GetBranchAsync(branch.ExternalPlaceId, cancellationToken);
            var validation = Validate(externalData, branch.ExternalPlaceId);

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var recordsStored = 0;
            if (externalData.Provider == PublicDataProvider.Demo)
            {
                recordsStored += await PersistSnapshotAsync(branch, run, externalData, cancellationToken);
                recordsStored += await UpsertReviewsAsync(branch, externalData, cancellationToken);
            }

            run.FinishedAtUtc = DateTimeOffset.UtcNow;
            run.Status = validation.IsPartial ? IntegrationRunStatus.Partial : IntegrationRunStatus.Successful;
            run.RecordsReceived = 1 + externalData.Reviews.Count;
            run.RecordsStored = recordsStored;
            run.ErrorCode = validation.IsPartial ? "INT-422-PARTIAL" : null;
            run.UserMessage = validation.IsPartial
                ? "La sincronizacion finalizo con datos parciales."
                : "La sincronizacion finalizo correctamente.";
            run.TechnicalMessage = validation.TechnicalMessage;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ToResult(branch, run);
        }
        catch (PublicDataProviderException exception)
        {
            logger.LogWarning(
                exception,
                "Branch sync failed {BranchId} {ErrorCode} {CorrelationId}",
                branch.Id,
                exception.ErrorCode,
                run.CorrelationId);

            run.FinishedAtUtc = DateTimeOffset.UtcNow;
            run.Status = IntegrationRunStatus.Failed;
            run.ErrorCode = exception.ErrorCode;
            run.UserMessage = exception.UserMessage;
            run.TechnicalMessage = exception.TechnicalMessage;
            await dbContext.SaveChangesAsync(cancellationToken);

            return ToResult(branch, run);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unexpected branch sync failure {BranchId} {CorrelationId}",
                branch.Id,
                run.CorrelationId);

            run.FinishedAtUtc = DateTimeOffset.UtcNow;
            run.Status = IntegrationRunStatus.Failed;
            run.ErrorCode = "INT-500-UNEXPECTED";
            run.UserMessage = "Ocurrio un error; use la correlacion para diagnosticar.";
            run.TechnicalMessage = exception.GetType().Name;
            await dbContext.SaveChangesAsync(cancellationToken);

            return ToResult(branch, run);
        }
    }

    public async Task<IReadOnlyList<SynchronizationResult>> SynchronizeAllActiveBranchesAsync(
        string triggeredByUserId,
        CancellationToken cancellationToken)
    {
        var branchIds = await dbContext.Branches
            .Where(branch => branch.IsActive)
            .OrderBy(branch => branch.Code)
            .Select(branch => branch.Id)
            .ToListAsync(cancellationToken);

        var results = new List<SynchronizationResult>();
        foreach (var branchId in branchIds)
        {
            results.Add(await SynchronizeBranchAsync(branchId, triggeredByUserId, cancellationToken));
        }

        return results;
    }

    private async Task<SynchronizationResult> CreateRejectedRunAsync(
        Branch branch,
        string triggeredByUserId,
        string errorCode,
        string userMessage,
        string technicalMessage,
        CancellationToken cancellationToken)
    {
        var run = new IntegrationRun
        {
            Id = Guid.NewGuid(),
            CorrelationId = CreateCorrelationId(),
            Provider = branch.Provider,
            BranchId = branch.Id,
            StartedAtUtc = DateTimeOffset.UtcNow,
            FinishedAtUtc = DateTimeOffset.UtcNow,
            Status = IntegrationRunStatus.Failed,
            ErrorCode = errorCode,
            UserMessage = userMessage,
            TechnicalMessage = technicalMessage,
            TriggeredByUserId = triggeredByUserId
        };

        dbContext.IntegrationRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResult(branch, run);
    }

    private async Task<int> PersistSnapshotAsync(
        Branch branch,
        IntegrationRun run,
        ExternalBranchData externalData,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.BranchSnapshots.AnyAsync(
            snapshot =>
                snapshot.BranchId == branch.Id &&
                snapshot.Provider == externalData.Provider &&
                snapshot.RetrievedAtUtc == externalData.RetrievedAtUtc,
            cancellationToken);

        if (exists)
        {
            return 0;
        }

        dbContext.BranchSnapshots.Add(new BranchSnapshot
        {
            Id = Guid.NewGuid(),
            BranchId = branch.Id,
            Provider = externalData.Provider,
            DisplayName = externalData.DisplayName,
            Address = externalData.Address,
            Latitude = externalData.Latitude,
            Longitude = externalData.Longitude,
            BusinessStatus = externalData.BusinessStatus,
            OpeningHoursJson = JsonSerializer.Serialize(externalData.OpeningHoursText),
            Rating = externalData.Rating,
            ReviewCount = externalData.ReviewCount,
            RetrievedAtUtc = externalData.RetrievedAtUtc,
            IntegrationRunId = run.Id
        });

        return 1;
    }

    private async Task<int> UpsertReviewsAsync(
        Branch branch,
        ExternalBranchData externalData,
        CancellationToken cancellationToken)
    {
        var stored = 0;
        foreach (var externalReview in externalData.Reviews)
        {
            var review = await dbContext.Reviews.SingleOrDefaultAsync(
                candidate =>
                    candidate.Provider == externalData.Provider &&
                    candidate.ExternalReviewId == externalReview.ExternalReviewId,
                cancellationToken);

            if (review is null)
            {
                dbContext.Reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    BranchId = branch.Id,
                    Provider = externalData.Provider,
                    ExternalReviewId = externalReview.ExternalReviewId,
                    Rating = externalReview.Rating,
                    Text = externalReview.Text,
                    PublishedAtUtc = externalReview.PublishedAtUtc,
                    AuthorDisplayName = externalReview.AuthorDisplayName,
                    Language = externalReview.Language,
                    SourceUrl = externalReview.SourceUrl,
                    RetrievedAtUtc = externalData.RetrievedAtUtc
                });
                stored++;
            }
            else
            {
                review.Rating = externalReview.Rating;
                review.Text = externalReview.Text;
                review.PublishedAtUtc = externalReview.PublishedAtUtc;
                review.AuthorDisplayName = externalReview.AuthorDisplayName;
                review.Language = externalReview.Language;
                review.SourceUrl = externalReview.SourceUrl;
                review.RetrievedAtUtc = externalData.RetrievedAtUtc;
            }
        }

        return stored;
    }

    private static CanonicalValidationResult Validate(ExternalBranchData externalData, string requestedExternalPlaceId)
    {
        var messages = new List<string>();

        if (externalData.Provider == 0 || string.IsNullOrWhiteSpace(externalData.ExternalPlaceId))
        {
            throw new PublicDataProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", "Provider or external id is empty.");
        }

        if (!string.Equals(externalData.ExternalPlaceId, requestedExternalPlaceId, StringComparison.Ordinal))
        {
            throw new PublicDataProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", "External id did not match requested id.");
        }

        if (externalData.Rating is null)
        {
            messages.Add("Rating is missing or invalid.");
        }

        if (externalData.ReviewCount is null)
        {
            messages.Add("Review count is missing or invalid.");
        }

        if (externalData.Reviews.Count == 0)
        {
            messages.Add("No valid reviews were available.");
        }

        return new CanonicalValidationResult(messages.Count > 0, string.Join(" ", messages));
    }

    private static SynchronizationResult ToResult(Branch branch, IntegrationRun run)
    {
        return new SynchronizationResult(
            branch.Id,
            branch.Code,
            branch.Name,
            run.Id,
            run.CorrelationId,
            run.Status,
            run.RecordsReceived,
            run.RecordsStored,
            run.UserMessage ?? "No disponible",
            run.ErrorCode);
    }

    private static string CreateCorrelationId()
    {
        return $"SYNC-{Guid.NewGuid():N}"[..32];
    }

    private sealed record CanonicalValidationResult(bool IsPartial, string TechnicalMessage);
}
