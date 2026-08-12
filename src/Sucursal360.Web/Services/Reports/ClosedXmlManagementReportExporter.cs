using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Services.Reports;

public sealed class ClosedXmlManagementReportExporter(ApplicationDbContext dbContext) : IManagementReportExporter
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<ManagementReportResult> ExportAsync(ManagementReportRequest request, CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new BranchReportRow(branch.Id, branch.Code, branch.Name, branch.Provider.ToString()))
            .ToListAsync(cancellationToken);

        var categories = await dbContext.ReviewCategories
            .OrderBy(category => category.Name)
            .Select(category => new CategoryReportRow(category.Id, category.Code, category.Name))
            .ToListAsync(cancellationToken);

        var branchIds = request.BranchId is null
            ? branches.Select(branch => branch.Id).ToHashSet()
            : branches.Where(branch => branch.Id == request.BranchId).Select(branch => branch.Id).ToHashSet();

        var snapshots = await dbContext.BranchSnapshots
            .Where(snapshot => branchIds.Contains(snapshot.BranchId))
            .Select(snapshot => new SnapshotReportRow(
                snapshot.BranchId,
                snapshot.Provider.ToString(),
                snapshot.BusinessStatus,
                snapshot.Rating,
                snapshot.ReviewCount,
                snapshot.RetrievedAtUtc))
            .ToListAsync(cancellationToken);

        var reviews = await dbContext.Reviews
            .Where(review => branchIds.Contains(review.BranchId))
            .Select(review => new ReviewReportRow(
                review.Id,
                review.BranchId,
                review.Rating,
                review.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        var assignments = await dbContext.ReviewCategoryAssignments
            .Where(assignment => branchIds.Contains(assignment.Review.BranchId))
            .Select(assignment => new CategoryAssignmentReportRow(
                assignment.ReviewId,
                assignment.Review.BranchId,
                assignment.ReviewCategoryId,
                assignment.Review.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        var operationalMetrics = await dbContext.SimulatedOperationalMetrics
            .Where(metric => branchIds.Contains(metric.BranchId))
            .Select(metric => new OperationalReportRow(
                metric.BranchId,
                metric.BusinessDate,
                metric.NetSales,
                metric.TransactionCount,
                metric.Currency))
            .ToListAsync(cancellationToken);

        var filteredSnapshots = snapshots
            .Where(snapshot => IsInDateRange(DateOnly.FromDateTime(snapshot.RetrievedAtUtc.UtcDateTime), request))
            .ToList();
        var filteredReviews = reviews
            .Where(review => review.PublishedAtUtc is not null)
            .Where(review => IsInDateRange(DateOnly.FromDateTime(review.PublishedAtUtc!.Value.UtcDateTime), request))
            .ToList();
        var filteredAssignments = assignments
            .Where(assignment => request.CategoryId is null || assignment.CategoryId == request.CategoryId)
            .Where(assignment => assignment.PublishedAtUtc is not null)
            .Where(assignment => IsInDateRange(DateOnly.FromDateTime(assignment.PublishedAtUtc!.Value.UtcDateTime), request))
            .ToList();
        var filteredOperationalMetrics = operationalMetrics
            .Where(metric => IsInDateRange(metric.BusinessDate, request))
            .ToList();

        using var workbook = new XLWorkbook();
        var generatedAt = DateTimeOffset.UtcNow;
        AddSummarySheet(workbook, request, generatedAt, branches, categories, filteredSnapshots, filteredReviews, filteredOperationalMetrics);
        AddBranchesSheet(workbook, branches.Where(branch => branchIds.Contains(branch.Id)).ToList(), filteredSnapshots);
        AddTrendsSheet(workbook, branches, filteredSnapshots);
        AddCategoriesSheet(workbook, branches, categories, filteredAssignments);
        AddOperationalSheet(workbook, branches, filteredOperationalMetrics);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ManagementReportResult(
            CreateFileName(generatedAt),
            ContentType,
            stream.ToArray());
    }

    private static void AddSummarySheet(
        XLWorkbook workbook,
        ManagementReportRequest request,
        DateTimeOffset generatedAtUtc,
        IReadOnlyList<BranchReportRow> branches,
        IReadOnlyList<CategoryReportRow> categories,
        IReadOnlyList<SnapshotReportRow> snapshots,
        IReadOnlyList<ReviewReportRow> reviews,
        IReadOnlyList<OperationalReportRow> operationalMetrics)
    {
        var sheet = workbook.Worksheets.Add("Resumen");
        AddKeyValue(sheet, 1, "Generado", ToDisplayDateTime(generatedAtUtc));
        AddKeyValue(sheet, 2, "Periodo", $"{request.FromDate?.ToString("yyyy-MM-dd") ?? "Sin inicio"} a {request.ToDate?.ToString("yyyy-MM-dd") ?? "Sin fin"}");
        AddKeyValue(sheet, 3, "Filtro sucursal", branches.SingleOrDefault(branch => branch.Id == request.BranchId)?.Name ?? "Todas");
        AddKeyValue(sheet, 4, "Filtro categoria", categories.SingleOrDefault(category => category.Id == request.CategoryId)?.Name ?? "Todas");
        AddKeyValue(sheet, 5, "Fuente publica", "Demo fixtures locales");
        AddKeyValue(sheet, 6, "Operacion", "DATOS SIMULADOS");
        AddKeyValue(sheet, 7, "Snapshots incluidos", snapshots.Count.ToString("N0"));
        AddKeyValue(sheet, 8, "Resenas incluidas", reviews.Count.ToString("N0"));
        AddKeyValue(sheet, 9, "Metricas simuladas", operationalMetrics.Count.ToString("N0"));
        AddKeyValue(sheet, 10, "Advertencia", "Uso demo con datos ficticios; no implica causalidad entre reputacion y operacion.");
        FormatSummary(sheet);
    }

    private static void AddBranchesSheet(
        XLWorkbook workbook,
        IReadOnlyList<BranchReportRow> branches,
        IReadOnlyList<SnapshotReportRow> snapshots)
    {
        var sheet = workbook.Worksheets.Add("Sucursales");
        AddHeader(sheet, ["Codigo", "Sucursal", "Proveedor", "Rating", "Var. rating", "Resenas", "Var. resenas", "Estado", "Ultima actualizacion"]);

        var row = 2;
        foreach (var branch in branches)
        {
            var orderedSnapshots = snapshots
                .Where(snapshot => snapshot.BranchId == branch.Id)
                .OrderByDescending(snapshot => snapshot.RetrievedAtUtc)
                .ToList();
            var latest = orderedSnapshots.FirstOrDefault();
            var previous = orderedSnapshots.Skip(1).FirstOrDefault();

            sheet.Cell(row, 1).Value = Neutralize(branch.Code);
            sheet.Cell(row, 2).Value = Neutralize(branch.Name);
            sheet.Cell(row, 3).Value = Neutralize(branch.Provider);
            SetNullable(sheet.Cell(row, 4), latest?.Rating);
            SetNullable(sheet.Cell(row, 5), latest?.Rating is null || previous?.Rating is null ? null : latest.Rating.Value - previous.Rating.Value);
            SetNullable(sheet.Cell(row, 6), latest?.ReviewCount);
            SetNullable(sheet.Cell(row, 7), latest?.ReviewCount is null || previous?.ReviewCount is null ? null : latest.ReviewCount.Value - previous.ReviewCount.Value);
            sheet.Cell(row, 8).Value = Neutralize(FormatBusinessStatus(latest?.BusinessStatus));
            sheet.Cell(row, 9).Value = latest is null ? "No disponible" : ToDisplayDateTime(latest.RetrievedAtUtc);
            row++;
        }

        FormatTable(sheet, row - 1);
    }

    private static void AddTrendsSheet(
        XLWorkbook workbook,
        IReadOnlyList<BranchReportRow> branches,
        IReadOnlyList<SnapshotReportRow> snapshots)
    {
        var sheet = workbook.Worksheets.Add("Tendencias");
        AddHeader(sheet, ["Fecha", "Codigo", "Sucursal", "Proveedor", "Rating", "Resenas", "Estado"]);

        var row = 2;
        foreach (var snapshot in snapshots.OrderBy(snapshot => snapshot.RetrievedAtUtc))
        {
            var branch = branches.SingleOrDefault(branch => branch.Id == snapshot.BranchId);
            sheet.Cell(row, 1).Value = ToDisplayDateTime(snapshot.RetrievedAtUtc);
            sheet.Cell(row, 2).Value = Neutralize(branch?.Code ?? "No disponible");
            sheet.Cell(row, 3).Value = Neutralize(branch?.Name ?? "No disponible");
            sheet.Cell(row, 4).Value = Neutralize(snapshot.Provider);
            SetNullable(sheet.Cell(row, 5), snapshot.Rating);
            SetNullable(sheet.Cell(row, 6), snapshot.ReviewCount);
            sheet.Cell(row, 7).Value = Neutralize(FormatBusinessStatus(snapshot.BusinessStatus));
            row++;
        }

        FormatTable(sheet, row - 1);
    }

    private static void AddCategoriesSheet(
        XLWorkbook workbook,
        IReadOnlyList<BranchReportRow> branches,
        IReadOnlyList<CategoryReportRow> categories,
        IReadOnlyList<CategoryAssignmentReportRow> assignments)
    {
        var sheet = workbook.Worksheets.Add("Categorias");
        AddHeader(sheet, ["Codigo", "Sucursal", "Categoria", "Conteo"]);

        var row = 2;
        foreach (var group in assignments
            .GroupBy(assignment => new { assignment.BranchId, assignment.CategoryId })
            .OrderBy(group => branches.SingleOrDefault(branch => branch.Id == group.Key.BranchId)?.Code)
            .ThenBy(group => categories.SingleOrDefault(category => category.Id == group.Key.CategoryId)?.Name))
        {
            var branch = branches.SingleOrDefault(branch => branch.Id == group.Key.BranchId);
            var category = categories.SingleOrDefault(category => category.Id == group.Key.CategoryId);
            sheet.Cell(row, 1).Value = Neutralize(branch?.Code ?? "No disponible");
            sheet.Cell(row, 2).Value = Neutralize(branch?.Name ?? "No disponible");
            sheet.Cell(row, 3).Value = Neutralize(category?.Name ?? "No disponible");
            sheet.Cell(row, 4).Value = group.Count();
            row++;
        }

        FormatTable(sheet, row - 1);
    }

    private static void AddOperationalSheet(
        XLWorkbook workbook,
        IReadOnlyList<BranchReportRow> branches,
        IReadOnlyList<OperationalReportRow> metrics)
    {
        var sheet = workbook.Worksheets.Add("Operacion_Simulada");
        sheet.Cell(1, 1).Value = "DATOS SIMULADOS";
        sheet.Range(1, 1, 1, 7).Merge().Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightYellow);
        AddHeader(sheet, ["Fecha", "Codigo", "Sucursal", "Ventas netas", "Transacciones", "Ticket promedio", "Moneda"], 2);

        var row = 3;
        foreach (var metric in metrics.OrderBy(metric => metric.BusinessDate))
        {
            var branch = branches.SingleOrDefault(branch => branch.Id == metric.BranchId);
            sheet.Cell(row, 1).Value = metric.BusinessDate.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = Neutralize(branch?.Code ?? "No disponible");
            sheet.Cell(row, 3).Value = Neutralize(branch?.Name ?? "No disponible");
            sheet.Cell(row, 4).Value = metric.NetSales;
            sheet.Cell(row, 5).Value = metric.TransactionCount;
            SetNullable(sheet.Cell(row, 6), metric.TransactionCount == 0 ? null : metric.NetSales / metric.TransactionCount);
            sheet.Cell(row, 7).Value = Neutralize(metric.Currency);
            row++;
        }

        FormatTable(sheet, row - 1, 2);
    }

    private static void AddKeyValue(IXLWorksheet sheet, int row, string key, string value)
    {
        sheet.Cell(row, 1).Value = key;
        sheet.Cell(row, 2).Value = Neutralize(value);
    }

    private static void AddHeader(IXLWorksheet sheet, IReadOnlyList<string> headers, int row = 1)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            sheet.Cell(row, index + 1).Value = headers[index];
        }
    }

    private static void FormatSummary(IXLWorksheet sheet)
    {
        sheet.Columns().AdjustToContents();
        sheet.Column(1).Style.Font.SetBold();
    }

    private static void FormatTable(IXLWorksheet sheet, int lastRow, int headerRow = 1)
    {
        if (lastRow >= headerRow)
        {
            var lastColumn = sheet.LastColumnUsed()?.ColumnNumber() ?? 1;
            var header = sheet.Range(headerRow, 1, headerRow, lastColumn);
            header.Style.Font.SetBold();
            header.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#eef2f6"));
            sheet.Range(headerRow, 1, Math.Max(lastRow, headerRow), lastColumn).SetAutoFilter();
        }

        sheet.Columns().AdjustToContents();
    }

    private static void SetNullable(IXLCell cell, decimal? value)
    {
        cell.Value = value is null ? "No disponible" : value.Value;
    }

    private static void SetNullable(IXLCell cell, int? value)
    {
        cell.Value = value is null ? "No disponible" : value.Value;
    }

    private static bool IsInDateRange(DateOnly date, ManagementReportRequest request)
    {
        return (request.FromDate is null || date >= request.FromDate) &&
            (request.ToDate is null || date <= request.ToDate);
    }

    private static string Neutralize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value[0] is '=' or '+' or '-' or '@' ? $"'{value}" : value;
    }

    private static string CreateFileName(DateTimeOffset generatedAtUtc)
    {
        return $"Sucursal360_Reporte_{ToManaguaTime(generatedAtUtc):yyyyMMdd_HHmm}.xlsx";
    }

    private static string ToDisplayDateTime(DateTimeOffset value)
    {
        return ToManaguaTime(value).ToString("yyyy-MM-dd HH:mm");
    }

    private static DateTimeOffset ToManaguaTime(DateTimeOffset value)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
        return TimeZoneInfo.ConvertTime(value, timeZone);
    }

    private static string FormatBusinessStatus(BusinessStatus? businessStatus)
    {
        return businessStatus switch
        {
            BusinessStatus.Operational => "Operando",
            BusinessStatus.TemporarilyClosed => "Cierre temporal",
            BusinessStatus.PermanentlyClosed => "Cierre permanente",
            BusinessStatus.Unknown => "Desconocido",
            _ => "No disponible"
        };
    }

    private sealed record BranchReportRow(Guid Id, string Code, string Name, string Provider);

    private sealed record CategoryReportRow(Guid Id, string Code, string Name);

    private sealed record SnapshotReportRow(
        Guid BranchId,
        string Provider,
        BusinessStatus? BusinessStatus,
        decimal? Rating,
        int? ReviewCount,
        DateTimeOffset RetrievedAtUtc);

    private sealed record ReviewReportRow(
        Guid Id,
        Guid BranchId,
        byte? Rating,
        DateTimeOffset? PublishedAtUtc);

    private sealed record CategoryAssignmentReportRow(
        Guid ReviewId,
        Guid BranchId,
        Guid CategoryId,
        DateTimeOffset? PublishedAtUtc);

    private sealed record OperationalReportRow(
        Guid BranchId,
        DateOnly BusinessDate,
        decimal NetSales,
        int TransactionCount,
        string Currency);
}
