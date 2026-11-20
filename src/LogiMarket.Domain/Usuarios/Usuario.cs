using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Usuarios;

public class Usuario
{
    public int Id { get; private set; }
    public string NombreUsuario { get; private set; } = null!;
    public byte[] PasswordHash { get; private set; } = null!;
    public byte[] Salt { get; private set; } = null!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }

    private Usuario() { }

    public Usuario(string nombreUsuario, string password, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new DomainException("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("La contraseña es obligatoria.");

        NombreUsuario = nombreUsuario;
        Rol = rol;
        Activo = true;

        var (hash, salt) = PasswordHasher.HashPassword(password);
        PasswordHash = hash;
        Salt = salt;
    }

    public bool ValidarPassword(string password)
        => PasswordHasher.VerifyPassword(password, PasswordHash, Salt);

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
