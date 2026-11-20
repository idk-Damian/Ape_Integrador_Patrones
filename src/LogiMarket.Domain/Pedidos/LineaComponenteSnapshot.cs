namespace LogiMarket.Domain.Pedidos;

/// <summary>Snapshot de un componente físico dentro de una línea de pedido (kit expandido).</summary>
public class LineaComponenteSnapshot
{
    public int Id { get; private set; }
    public int LineaPedidoId { get; private set; }
    public string ProductoCodigo { get; private set; } = null!;
    public string ProductoNombre { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PesoKg { get; private set; }

    private LineaComponenteSnapshot() { }

    public LineaComponenteSnapshot(string productoCodigo, string productoNombre,
        decimal precioUnitario, int cantidad, decimal pesoKg)
    {
        ProductoCodigo = productoCodigo;
        ProductoNombre = productoNombre;
        PrecioUnitario = precioUnitario;
        Cantidad = cantidad;
        PesoKg = pesoKg;
    }

    internal LineaComponenteSnapshot ClonarProfundo()
        => new(ProductoCodigo, ProductoNombre, PrecioUnitario, Cantidad, PesoKg);
}
