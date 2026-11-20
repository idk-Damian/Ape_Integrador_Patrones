namespace LogiMarket.Domain.Pedidos.Memento;

/// <summary>
/// Patrón MEMENTO – Snapshot interno inmutable de un pedido en borrador.
/// Almacena copia profunda de líneas con kits anidados.
/// </summary>
public sealed class PedidoMemento
{
    internal int ClienteId { get; }
    internal string TipoEntrega { get; }
    internal decimal Subtotal { get; }
    internal decimal Descuento { get; }
    internal string NombreDescuento { get; }
    internal decimal TarifaEnvio { get; }
    internal decimal Total { get; }
    internal IReadOnlyList<LineaSnapshot> Lineas { get; }

    internal PedidoMemento(int clienteId, string tipoEntrega,
        decimal subtotal, decimal descuento, string nombreDescuento,
        decimal tarifaEnvio, decimal total, IReadOnlyList<LineaSnapshot> lineas)
    {
        ClienteId = clienteId;
        TipoEntrega = tipoEntrega;
        Subtotal = subtotal;
        Descuento = descuento;
        NombreDescuento = nombreDescuento;
        TarifaEnvio = tarifaEnvio;
        Total = total;
        Lineas = lineas;
    }

    internal sealed record LineaSnapshot(
        string ElementoCodigo, string ElementoNombre, decimal PrecioUnitario,
        int Cantidad, bool EsKit,
        IReadOnlyList<ComponenteSnap> Componentes);

    internal sealed record ComponenteSnap(
        string Codigo, string Nombre, decimal PrecioUnitario, int Cantidad, decimal PesoKg);
}
