using LogiMarket.Domain.Despachos;

namespace LogiMarket.Application.Interfaces;

public interface IDespachoRepository
{
    Task<Despacho?> ObtenerPorPedidoAsync(int pedidoId);
    Task AgregarAsync(Despacho despacho);
}
