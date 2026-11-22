using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Estados;

// convierte el texto guardado en la base al objeto de estado correspondiente
public static class EstadoPedidoFactory
{
    private static readonly Dictionary<string, IEstadoPedido> Estados = new IEstadoPedido[]
    {
        EstadoBorrador.Instancia,
        EstadoValidado.Instancia,
        EstadoPagado.Instancia,
        EstadoDespachado.Instancia,
        EstadoEntregado.Instancia,
        EstadoCancelado.Instancia
    }.ToDictionary(e => e.Nombre);

    public static IEstadoPedido Crear(string nombre)
        => Estados.TryGetValue(nombre, out var estado)
            ? estado
            : throw new DomainException($"Estado de pedido desconocido: '{nombre}'.");
}
