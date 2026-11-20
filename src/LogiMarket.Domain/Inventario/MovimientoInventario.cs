using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Inventario;

public class MovimientoInventario
{
    public int Id { get; private set; }
    public int ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public int? PedidoId { get; private set; }
    public DateTime Fecha { get; private set; }

    private MovimientoInventario() { }

    public MovimientoInventario(int productoId, int cantidad, TipoMovimiento tipo, int? pedidoId = null)
    {
        if (productoId <= 0) throw new DomainException("ID de producto inválido.");
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser positiva.");

        ProductoId = productoId;
        Cantidad = cantidad;
        Tipo = tipo;
        PedidoId = pedidoId;
        Fecha = DateTime.UtcNow;
    }
}
