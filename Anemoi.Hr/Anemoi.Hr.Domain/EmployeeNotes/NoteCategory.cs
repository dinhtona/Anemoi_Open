namespace Anemoi.Hr.Domain.EmployeeNotes;

public sealed record NoteCategory(string Value)
{
    public static readonly NoteCategory General = new("general");
    public static readonly NoteCategory Performance = new("performance");
    public static readonly NoteCategory Disciplinary = new("disciplinary");
    public static readonly NoteCategory Personal = new("personal");

    private static readonly IReadOnlyCollection<NoteCategory> All =
    [General, Performance, Disciplinary, Personal];

    public static NoteCategory FromValue(string value) =>
        All.FirstOrDefault(c => c.Value == value) ?? throw new ArgumentException($"Unknown NoteCategory: {value}");

    public override string ToString() => Value;
}
