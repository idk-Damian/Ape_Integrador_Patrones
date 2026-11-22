using System.Security.Cryptography;
using LogiMarket.Application.Interfaces;

namespace LogiMarket.Infrastructure.Seguridad;

// PBKDF2 con SHA-256 y sal aleatoria; nunca se guarda la contrasena en claro
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int Iteraciones = 100_000;

    public (byte[] Hash, byte[] Salt) Generar(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("La contraseña es obligatoria.");
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        return (Derivar(password, sal), sal);
    }

    public bool Verificar(string password, byte[] hash, byte[] salt)
        => !string.IsNullOrEmpty(password) && CryptographicOperations.FixedTimeEquals(Derivar(password, salt), hash);

    private static byte[] Derivar(string password, byte[] sal)
        => Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
}
