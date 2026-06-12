namespace O2Connect.Api.Models.SmartEnums;

public readonly record struct UserRole : ISmartEnum<UserRole>
{
    private static readonly UserRole None = new(string.Empty);
    
    public static readonly UserRole Admin = new("admin");
    public static readonly UserRole User = new("user");
    public static readonly UserRole Service = new("service");
    public static readonly UserRole Developer = new("developer");
    public static readonly UserRole Support = new("support");
    public static readonly UserRole Auditor = new("auditor");
    public static readonly UserRole Manager = new("manager");
    public static readonly UserRole Operator = new("operator");

    public static IReadOnlyList<UserRole> Supported { get; } =
    [
        Admin,
        User,
        Service,
        Developer,
        Support,
        Auditor,
        Manager,
        Operator
    ];

    public string Value { get; }

    private UserRole(string value)
    {
        Value = value;
    }

    public static implicit operator string(UserRole role) => role.Value;

    public static bool TryParse(string? value, out UserRole result)
    {
        result = None;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim().ToLowerInvariant();

        result = value switch
        {
            "admin" => Admin,
            "user" => User,
            "service" => Service,
            "developer" => Developer,
            "support" => Support,
            "auditor" => Auditor,
            "manager" => Manager,
            "operator" => Operator,
            _ => None
        };

        return result != None;
    }

    public override string ToString()
    {
        return Value;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
