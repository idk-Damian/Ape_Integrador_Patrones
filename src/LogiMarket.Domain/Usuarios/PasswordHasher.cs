using System.Security.Cryptography;

namespace LogiMarket.Domain.Usuarios;

/// <summary>
/// Hasheador de contraseñas con PBKDF2 (Rfc2898DeriveBytes). NUNCA texto plano.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return (hash, salt);
    }

    public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, Iterations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }
}
