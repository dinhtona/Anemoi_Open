namespace Anemoi.Hr.Domain.EmployeeDocuments;

public sealed record DocumentType(string Value)
{
    public static readonly DocumentType LaborContract = new("labor_contract");
    public static readonly DocumentType IdCard = new("id_card");
    public static readonly DocumentType Passport = new("passport");
    public static readonly DocumentType Visa = new("visa");
    public static readonly DocumentType Certificate = new("certificate");
    public static readonly DocumentType Education = new("education");
    public static readonly DocumentType Resume = new("resume");
    public static readonly DocumentType Other = new("other");

    private static readonly IReadOnlyCollection<DocumentType> All =
    [
        LaborContract, IdCard, Passport, Visa, Certificate, Education, Resume, Other
    ];

    public static DocumentType FromValue(string value) =>
        All.FirstOrDefault(t => t.Value == value) ?? throw new ArgumentException($"Unknown DocumentType: {value}");

    public override string ToString() => Value;
}
