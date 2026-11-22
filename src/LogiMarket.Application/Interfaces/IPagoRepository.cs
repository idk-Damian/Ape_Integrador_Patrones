using LogiMarket.Domain.Pagos;

namespace LogiMarket.Application.Interfaces;

// sin eliminar ni actualizar: un cobro registrado no se toca
public interface IPagoRepository
{
    Task<Pago?> ObtenerPorClaveIdempotenciaAsync(string clave);
    Task<Pago?> ObtenerPorPedidoAsync(int pedidoId);
    Task AgregarAsync(Pago pago);
}
