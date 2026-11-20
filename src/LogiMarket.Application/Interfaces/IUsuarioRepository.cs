using LogiMarket.Domain.Usuarios;

namespace LogiMarket.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);
}
