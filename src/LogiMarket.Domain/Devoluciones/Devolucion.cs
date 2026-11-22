using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Devoluciones;

// solo se registran devoluciones aceptadas; los rechazos quedan en auditoria
public class Devolucion
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public MotivoDevolucion Motivo { get; private set; }
    public int DiasDesdeEntrega { get; private set; }
    public bool ProductoSinUso { get; private set; }
    public decimal MontoReembolso { get; private set; }
    public string ReferenciaReembolso { get; private set; } = null!;
    public DateTime Fecha { get; private set; }

    private Devolucion() { }

    public Devolucion(int pedidoId, MotivoDevolucion motivo, int diasDesdeEntrega, bool productoSinUso,
        decimal montoReembolso, string referenciaReembolso, DateTime fecha)
    {
        if (pedidoId <= 0) throw new DomainException("ID de pedido inválido.");
        if (diasDesdeEntrega < 0) throw new DomainException("Los días desde la entrega no pueden ser negativos.");
        if (montoReembolso <= 0) throw new DomainException("El reembolso debe ser positivo.");
        if (string.IsNullOrWhiteSpace(referenciaReembolso))
            throw new DomainException("La referencia del reembolso es obligatoria.");

        PedidoId = pedidoId;
        Motivo = motivo;
        DiasDesdeEntrega = diasDesdeEntrega;
        ProductoSinUso = productoSinUso;
        MontoReembolso = Moneda.Redondear(montoReembolso);
        ReferenciaReembolso = referenciaReembolso;
        Fecha = fecha;
    }
}
