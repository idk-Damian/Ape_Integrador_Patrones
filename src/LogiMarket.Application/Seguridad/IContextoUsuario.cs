using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Seguridad;

public interface IContextoUsuario
{
    bool Autenticado { get; }
    string NombreUsuario { get; }
    RolUsuario? Rol { get; }
}
