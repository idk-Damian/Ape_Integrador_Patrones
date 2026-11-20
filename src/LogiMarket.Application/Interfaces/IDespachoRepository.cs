using LogiMarket.Domain.Despachos;

namespace LogiMarket.Application.Interfaces;

public interface IDespachoRepository : IRepository<Despacho>
{
    Task<Despacho?> ObtenerPorPedidoAsync(int pedidoId);
}
