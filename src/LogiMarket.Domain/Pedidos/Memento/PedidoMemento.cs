using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Memento;

// foto inmutable del borrador; solo el pedido sabe leerla
public sealed class PedidoMemento
{
    internal PedidoMemento(TipoEntrega tipoEntrega, decimal tarifaEnvio, IReadOnlyList<LineaPedido> lineas)
    {
        TipoEntrega = tipoEntrega;
        TarifaEnvio = tarifaEnvio;
        Lineas = lineas;
        Fecha = DateTime.UtcNow;
    }

    internal TipoEntrega TipoEntrega { get; }
    internal decimal TarifaEnvio { get; }
    internal IReadOnlyList<LineaPedido> Lineas { get; }
    public DateTime Fecha { get; }
}
