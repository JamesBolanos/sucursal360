namespace Sucursal360.Web.Services.SimulatedData;

public sealed record SimulatedCsvValidationResult(
    string FileName,
    IReadOnlyList<SimulatedCsvRow> Rows,
    IReadOnlyList<SimulatedCsvValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public int RowCount => Rows.Count;

    public DateOnly? PeriodStart => Rows.Count == 0 ? null : Rows.Min(row => row.BusinessDate);

    public DateOnly? PeriodEnd => Rows.Count == 0 ? null : Rows.Max(row => row.BusinessDate);

    public IReadOnlyList<string> BranchCodes => Rows
        .Select(row => row.BranchCode)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();
}
