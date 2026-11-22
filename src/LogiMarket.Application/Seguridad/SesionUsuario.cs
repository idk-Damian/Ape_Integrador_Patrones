using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Seguridad;

// usuario de la sesion actual de escritorio
public sealed class SesionUsuario : IContextoUsuario
{
    public bool Autenticado => Rol is not null;
    public string NombreUsuario { get; private set; } = "anonimo";
    public RolUsuario? Rol { get; private set; }

    public void Iniciar(string nombreUsuario, RolUsuario rol)
    {
        NombreUsuario = nombreUsuario;
        Rol = rol;
    }

    public void Cerrar()
    {
        NombreUsuario = "anonimo";
        Rol = null;
    }
}
