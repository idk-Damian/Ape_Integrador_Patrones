using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Inventario;

// cantidad con signo: positiva entra, negativa sale
public class MovimientoInventario
{
    public int Id { get; private set; }
    public int ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public int StockResultante { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public string Motivo { get; private set; } = null!;
    public string Referencia { get; private set; } = null!;
    public int? PedidoId { get; private set; }
    public DateTime Fecha { get; private set; }

    private MovimientoInventario() { }

    public MovimientoInventario(int productoId, int cantidad, int stockResultante, TipoMovimiento tipo,
        string motivo, string referencia, DateTime fecha, int? pedidoId = null)
    {
        if (productoId <= 0) throw new DomainException("ID de producto inválido.");
        if (cantidad == 0) throw new DomainException("Un movimiento no puede tener cantidad cero.");
        if (EsEntrada(tipo) != cantidad > 0)
            throw new DomainException($"El signo de la cantidad no corresponde al movimiento {tipo}.");
        if (stockResultante < 0) throw new DomainException("El stock resultante no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new DomainException("El motivo del movimiento es obligatorio.");
        if (string.IsNullOrWhiteSpace(referencia)) throw new DomainException("La referencia del movimiento es obligatoria.");

        ProductoId = productoId;
        Cantidad = cantidad;
        StockResultante = stockResultante;
        Tipo = tipo;
        Motivo = motivo;
        Referencia = referencia;
        Fecha = fecha;
        PedidoId = pedidoId;
    }

    public static bool EsEntrada(TipoMovimiento tipo)
        => tipo is TipoMovimiento.InventarioInicial or TipoMovimiento.ReingresoDevolucion or TipoMovimiento.AjusteEntrada;
}
