using LogiMarket.Domain.Devoluciones;

namespace LogiMarket.Application.Interfaces;

public interface IDevolucionRepository : IRepository<Devolucion>
{
    Task<Devolucion?> ObtenerPorPedidoAsync(int pedidoId);
    Task<bool> ExisteDevolucionParaPedidoAsync(int pedidoId);
}
