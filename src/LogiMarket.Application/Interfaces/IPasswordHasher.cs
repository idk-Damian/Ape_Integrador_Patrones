namespace LogiMarket.Application.Interfaces;

// puerto de seguridad; el algoritmo concreto vive en Infrastructure
public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt) Generar(string password);
    bool Verificar(string password, byte[] hash, byte[] salt);
}
