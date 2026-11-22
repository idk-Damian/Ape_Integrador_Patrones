namespace LogiMarket.Domain.Pedidos;

// SKU final que forma parte de una linea, con el precio que tenia al venderse
public class LineaComponenteSnapshot
{
    public int Id { get; private set; }
    public int LineaPedidoId { get; private set; }
    public string ProductoCodigo { get; private set; } = null!;
    public string ProductoNombre { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PesoKg { get; private set; }
    public int Orden { get; private set; }

    private LineaComponenteSnapshot() { }

    internal LineaComponenteSnapshot(string productoCodigo, string productoNombre,
        decimal precioUnitario, int cantidad, decimal pesoKg, int orden)
    {
        ProductoCodigo = productoCodigo;
        ProductoNombre = productoNombre;
        PrecioUnitario = precioUnitario;
        Cantidad = cantidad;
        PesoKg = pesoKg;
        Orden = orden;
    }

    internal LineaComponenteSnapshot Clonar()
        => new(ProductoCodigo, ProductoNombre, PrecioUnitario, Cantidad, PesoKg, Orden);
}
