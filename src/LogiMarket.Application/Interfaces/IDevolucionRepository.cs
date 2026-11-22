using LogiMarket.Domain.Devoluciones;

namespace LogiMarket.Application.Interfaces;

public interface IDevolucionRepository
{
    Task<Devolucion?> ObtenerPorPedidoAsync(int pedidoId);
    Task AgregarAsync(Devolucion devolucion);
}
