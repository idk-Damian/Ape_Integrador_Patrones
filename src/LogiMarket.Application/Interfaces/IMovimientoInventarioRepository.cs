using LogiMarket.Domain.Inventario;

namespace LogiMarket.Application.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task AgregarAsync(MovimientoInventario movimiento);
    Task<IReadOnlyList<MovimientoInventario>> ListarPorPedidoAsync(int pedidoId);
    Task<IReadOnlyList<MovimientoInventario>> ListarAsync(int? productoId = null);
}
