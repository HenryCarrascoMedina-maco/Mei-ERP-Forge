using System.Security.Cryptography;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Password hashing and verification using PBKDF2 (HMAC-SHA256). No external dependencies.
/// The stored hash is self-describing: "{iterations}.{saltBase64}.{hashBase64}".
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int DefaultIterations = 100_000;

    /// <summary>Hashes a plaintext password into a storable string.</summary>
    public static string Hash(string password, int iterations = DefaultIterations)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    /// <summary>Verifies a plaintext password against a stored hash. Constant-time comparison.</summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
