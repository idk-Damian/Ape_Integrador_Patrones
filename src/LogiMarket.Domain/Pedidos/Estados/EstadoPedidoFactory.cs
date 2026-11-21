using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>
/// Fábrica de estados usando diccionario (evita switch gigante).
/// </summary>
public static class EstadoPedidoFactory
{
    private static readonly Dictionary<string, IEstadoPedido> Estados = new()
    {
        ["BORRADOR"] = EstadoBorrador.Instancia,
        ["VALIDADO"] = EstadoValidado.Instancia,
        ["PAGADO"] = EstadoPagado.Instancia,
        ["DESPACHADO"] = EstadoDespachado.Instancia,
        ["ENTREGADO"] = EstadoEntregado.Instancia,
        ["CANCELADO"] = EstadoCancelado.Instancia,
    };

    public static IEstadoPedido Crear(string nombre)
        => Estados.TryGetValue(nombre, out var estado)
            ? estado
            : throw new DomainException($"Estado de pedido desconocido: '{nombre}'.");
}
