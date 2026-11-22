using LogiMarket.Domain.Usuarios;

namespace LogiMarket.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);
    Task AgregarAsync(Usuario usuario);
}
