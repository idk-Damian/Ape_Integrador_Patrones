using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pagos;

// solo se guarda el cobro aprobado; un rechazo revierte la transaccion completa
public class Pago
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public decimal Monto { get; private set; }
    public string Proveedor { get; private set; } = null!;
    public string ReferenciaCobro { get; private set; } = null!;
    public string ClaveIdempotencia { get; private set; } = null!;
    public DateTime Fecha { get; private set; }

    private Pago() { }

    public Pago(int pedidoId, decimal monto, string proveedor, string referenciaCobro, string claveIdempotencia, DateTime fecha)
    {
        if (pedidoId <= 0) throw new DomainException("El ID del pedido es inválido.");
        if (monto <= 0) throw new DomainException("El monto debe ser positivo.");
        if (string.IsNullOrWhiteSpace(proveedor)) throw new DomainException("El proveedor de pago es obligatorio.");
        if (string.IsNullOrWhiteSpace(referenciaCobro)) throw new DomainException("La referencia de cobro es obligatoria.");
        if (string.IsNullOrWhiteSpace(claveIdempotencia)) throw new DomainException("La clave de idempotencia es obligatoria.");

        PedidoId = pedidoId;
        Monto = Moneda.Redondear(monto);
        Proveedor = proveedor;
        ReferenciaCobro = referenciaCobro;
        ClaveIdempotencia = claveIdempotencia.Trim();
        Fecha = fecha;
    }
}
