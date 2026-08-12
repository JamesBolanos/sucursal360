using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Services.SimulatedData;

public sealed class CsvSimulatedDataImportService(ApplicationDbContext dbContext) : ISimulatedDataImportService
{
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;
    private const int MaxRows = 10_000;
    private static readonly string[] ExpectedHeader =
    [
        "business_date",
        "branch_code",
        "net_sales",
        "transaction_count",
        "currency",
        "data_origin"
    ];

    public async Task<SimulatedCsvValidationResult> ValidateAsync(
        string csvContent,
        string fileName,
        long fileSizeBytes,
        CancellationToken cancellationToken)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        if (fileSizeBytes > MaxFileSizeBytes)
        {
            return new SimulatedCsvValidationResult(
                sanitizedFileName,
                [],
                [new SimulatedCsvValidationError(1, "file", "CSV-413-SIZE", "El archivo supera el limite de 2 MB.")]);
        }

        var branches = await dbContext.Branches
            .Select(branch => new BranchLookup(branch.Id, branch.Code, branch.Name))
            .ToDictionaryAsync(branch => branch.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var errors = new List<SimulatedCsvValidationError>();
        var rows = new List<SimulatedCsvRow>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = ReadLines(csvContent).ToList();

        if (lines.Count == 0 || string.IsNullOrWhiteSpace(lines[0]))
        {
            return new SimulatedCsvValidationResult(
                sanitizedFileName,
                [],
                [new SimulatedCsvValidationError(1, "header", "CSV-400-HEADER", "El archivo no contiene encabezado.")]);
        }

        var header = ParseCsvLine(lines[0]);
        if (!HeaderMatches(header))
        {
            return new SimulatedCsvValidationResult(
                sanitizedFileName,
                [],
                [new SimulatedCsvValidationError(1, "header", "CSV-400-HEADER", "El encabezado no coincide con el contrato CSV.")]);
        }

        var dataLines = lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (dataLines.Count > MaxRows)
        {
            errors.Add(new SimulatedCsvValidationError(1, "file", "CSV-413-SIZE", "El archivo supera el limite de 10000 filas."));
        }

        if (dataLines.Count == 0)
        {
            errors.Add(new SimulatedCsvValidationError(2, "row", "CSV-422-ROW", "El archivo no contiene filas para importar."));
        }

        var rowLimit = Math.Min(dataLines.Count, MaxRows);
        for (var dataIndex = 0; dataIndex < rowLimit; dataIndex++)
        {
            var line = dataLines[dataIndex];
            var rowNumber = dataIndex + 2;
            var fields = ParseCsvLine(line);
            if (fields.Count != ExpectedHeader.Length)
            {
                errors.Add(new SimulatedCsvValidationError(rowNumber, "row", "CSV-422-ROW", "La fila no contiene la cantidad esperada de columnas."));
                continue;
            }

            var businessDate = ParseBusinessDate(fields[0], rowNumber, errors);
            var branch = ParseBranch(fields[1], rowNumber, branches, errors);
            var netSales = ParseNetSales(fields[2], rowNumber, errors);
            var transactionCount = ParseTransactionCount(fields[3], rowNumber, errors);
            var currency = ParseCurrency(fields[4], rowNumber, errors);
            ParseOrigin(fields[5], rowNumber, errors);

            if (businessDate is null || branch is null || netSales is null || transactionCount is null || currency is null)
            {
                continue;
            }

            var duplicateKey = $"{branch.Code}|{businessDate.Value:yyyy-MM-dd}";
            if (!seenKeys.Add(duplicateKey))
            {
                errors.Add(new SimulatedCsvValidationError(rowNumber, "branch_code,business_date", "CSV-422-DUPLICATE", "La sucursal y fecha estan duplicadas dentro del archivo."));
                continue;
            }

            rows.Add(new SimulatedCsvRow(
                rowNumber,
                branch.Id,
                branch.Code,
                branch.Name,
                businessDate.Value,
                netSales.Value,
                transactionCount.Value,
                currency));
        }

        return new SimulatedCsvValidationResult(sanitizedFileName, errors.Count == 0 ? rows : [], errors);
    }

    public async Task<SimulatedDataImportResult> ImportAsync(
        string csvContent,
        string fileName,
        long fileSizeBytes,
        string importedByUserId,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(csvContent, fileName, fileSizeBytes, cancellationToken);
        if (!validation.IsValid || validation.PeriodStart is null || validation.PeriodEnd is null)
        {
            throw new InvalidOperationException("CSV content is invalid and cannot be imported.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var import = new SimulatedDataImport
        {
            Id = Guid.NewGuid(),
            FileName = validation.FileName,
            RowCount = validation.RowCount,
            PeriodStart = validation.PeriodStart.Value,
            PeriodEnd = validation.PeriodEnd.Value,
            ImportedByUserId = importedByUserId,
            ImportedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.SimulatedDataImports.Add(import);

        foreach (var row in validation.Rows)
        {
            var metric = await dbContext.SimulatedOperationalMetrics.SingleOrDefaultAsync(
                candidate => candidate.BranchId == row.BranchId && candidate.BusinessDate == row.BusinessDate,
                cancellationToken);

            if (metric is null)
            {
                dbContext.SimulatedOperationalMetrics.Add(new SimulatedOperationalMetric
                {
                    Id = Guid.NewGuid(),
                    BranchId = row.BranchId,
                    BusinessDate = row.BusinessDate,
                    NetSales = row.NetSales,
                    TransactionCount = row.TransactionCount,
                    Currency = row.Currency,
                    DataOrigin = DataOrigin.Simulated,
                    ImportId = import.Id
                });
            }
            else
            {
                metric.NetSales = row.NetSales;
                metric.TransactionCount = row.TransactionCount;
                metric.Currency = row.Currency;
                metric.DataOrigin = DataOrigin.Simulated;
                metric.ImportId = import.Id;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SimulatedDataImportResult(import.Id, import.FileName, import.RowCount, import.PeriodStart, import.PeriodEnd);
    }

    private static IReadOnlyList<string> ReadLines(string csvContent)
    {
        return csvContent
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static bool HeaderMatches(IReadOnlyList<string> header)
    {
        return header.Count == ExpectedHeader.Length &&
            header
                .Select(field => field.Trim())
                .SequenceEqual(ExpectedHeader, StringComparer.Ordinal);
    }

    private static DateOnly? ParseBusinessDate(string value, int rowNumber, List<SimulatedCsvValidationError> errors)
    {
        if (DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "business_date", "CSV-422-DATE", "La fecha debe usar formato yyyy-MM-dd."));
        return null;
    }

    private static BranchLookup? ParseBranch(
        string value,
        int rowNumber,
        IReadOnlyDictionary<string, BranchLookup> branches,
        List<SimulatedCsvValidationError> errors)
    {
        var branchCode = value.Trim().ToUpperInvariant();
        if (branches.TryGetValue(branchCode, out var branch))
        {
            return branch;
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "branch_code", "CSV-422-BRANCH", "El codigo de sucursal no existe."));
        return null;
    }

    private static decimal? ParseNetSales(string value, int rowNumber, List<SimulatedCsvValidationError> errors)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var netSales) && netSales >= 0)
        {
            return Math.Round(netSales, 2);
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "net_sales", "CSV-422-SALES", "La venta neta debe ser numerica y no negativa."));
        return null;
    }

    private static int? ParseTransactionCount(string value, int rowNumber, List<SimulatedCsvValidationError> errors)
    {
        if (int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var transactionCount) && transactionCount >= 0)
        {
            return transactionCount;
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "transaction_count", "CSV-422-TRANSACTIONS", "El conteo de transacciones debe ser entero y no negativo."));
        return null;
    }

    private static string? ParseCurrency(string value, int rowNumber, List<SimulatedCsvValidationError> errors)
    {
        var currency = value.Trim().ToUpperInvariant();
        if (currency == "NIO")
        {
            return currency;
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "currency", "CSV-422-CURRENCY", "La moneda debe ser NIO."));
        return null;
    }

    private static void ParseOrigin(string value, int rowNumber, List<SimulatedCsvValidationError> errors)
    {
        if (string.Equals(value.Trim(), "SIMULATED", StringComparison.Ordinal))
        {
            return;
        }

        errors.Add(new SimulatedCsvValidationError(rowNumber, "data_origin", "CSV-422-ORIGIN", "El origen debe ser SIMULATED."));
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Add('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                fields.Add(new string(current.ToArray()));
                current.Clear();
            }
            else
            {
                current.Add(character);
            }
        }

        fields.Add(new string(current.ToArray()));
        return fields;
    }

    private static string SanitizeFileName(string fileName)
    {
        var sanitized = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "simulated-data.csv";
        }

        return sanitized.Length <= 255 ? sanitized : sanitized[..255];
    }

    private sealed record BranchLookup(Guid Id, string Code, string Name);
}
