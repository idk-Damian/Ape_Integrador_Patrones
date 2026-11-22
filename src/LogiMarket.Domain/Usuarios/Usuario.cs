using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Usuarios;

// el hash lo calcula un servicio de infraestructura; aqui solo se guarda
public class Usuario
{
    public int Id { get; private set; }
    public string NombreUsuario { get; private set; } = null!;
    public byte[] PasswordHash { get; private set; } = null!;
    public byte[] Salt { get; private set; } = null!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }

    private Usuario() { }

    public Usuario(string nombreUsuario, byte[] passwordHash, byte[] salt, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new DomainException("El nombre de usuario es obligatorio.");
        if (passwordHash is null || passwordHash.Length == 0)
            throw new DomainException("El hash de la contraseña es obligatorio.");
        if (salt is null || salt.Length == 0)
            throw new DomainException("La sal de la contraseña es obligatoria.");

        NombreUsuario = nombreUsuario.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Salt = salt;
        Rol = rol;
        Activo = true;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
