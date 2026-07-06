using System.Security.Cryptography;
using System.Text;

namespace ErpBackend.Identity.Security;

/// <summary>
/// Deterministic SHA-256 hashing for refresh tokens at rest. Refresh tokens are high-entropy random
/// values, so a fast keyed-by-value hash (not a salted KDF) is the right tool: it lets us look the
/// token up by hash while never storing the plaintext.
/// </summary>
public static class TokenHasher
{
    public static string Hash(string token)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
