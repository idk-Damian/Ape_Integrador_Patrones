using LogiMarket.Domain.Inventario;

namespace LogiMarket.Application.Interfaces;

public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
{
    Task<IReadOnlyList<MovimientoInventario>> ObtenerPorProductoAsync(int productoId);
}
