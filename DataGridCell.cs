public class DataGridCell
{
    public required double X1 { get; init; }
    public required double Y1 { get; init; }
    public required double X2 { get; init; }
    public required double Y2 { get; init; }

    public required int Column { get; init; }
    public required int Row { get; init; }

    public required IEnumerable<string> ZoneCodes { get; init; }
}
