namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Configurable password rules, mirrored by the frontend `passwordValidator`. Bind from the
/// "PasswordPolicy" configuration section or use the defaults.
/// </summary>
public class PasswordPolicy
{
    public const string SectionName = "PasswordPolicy";

    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecial { get; set; } = true;

    /// <summary>Validates a password, returning the list of unmet rules (empty when valid).</summary>
    public IReadOnlyList<string> Validate(string? password)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Password must be at least {MinLength} characters.");
        }
        if (RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain an uppercase letter.");
        }
        if (RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain a lowercase letter.");
        }
        if (RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain a digit.");
        }
        if (RequireSpecial && password.All(char.IsLetterOrDigit))
        {
            errors.Add("Password must contain a special character.");
        }

        return errors;
    }

    public bool IsValid(string? password) => Validate(password).Count == 0;
}
