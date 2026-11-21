using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Devoluciones;

public class Devolucion
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public string Motivo { get; private set; } = null!;
    public decimal Monto { get; private set; }
    public DateTime Fecha { get; private set; }

    private Devolucion() { }

    public Devolucion(int pedidoId, string motivo, decimal monto)
    {
        if (pedidoId <= 0) throw new DomainException("ID de pedido inválido.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new DomainException("El motivo es obligatorio.");
        if (monto <= 0) throw new DomainException("El monto debe ser positivo.");

        PedidoId = pedidoId;
        Motivo = motivo;
        Monto = Moneda.Redondear(monto);
        Fecha = DateTime.UtcNow;
    }
}
