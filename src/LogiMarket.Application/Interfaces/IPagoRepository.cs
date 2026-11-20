using LogiMarket.Domain.Pagos;

namespace LogiMarket.Application.Interfaces;

public interface IPagoRepository : IRepository<Pago>
{
    Task<Pago?> ObtenerPorPedidoAsync(int pedidoId);
    Task<bool> ExisteClaveIdempotenciaAsync(string claveIdempotencia);
}
