namespace LogiMarket.Domain.Pedidos.Historial;

// bitacora de transiciones del pedido, solo se agrega, nunca se edita
public class CambioEstadoPedido
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public int Secuencia { get; private set; }
    public string? EstadoAnterior { get; private set; }
    public string EstadoNuevo { get; private set; } = null!;
    public DateTime Fecha { get; private set; }

    private CambioEstadoPedido() { }

    internal CambioEstadoPedido(int secuencia, string? estadoAnterior, string estadoNuevo, DateTime fecha)
    {
        Secuencia = secuencia;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        Fecha = fecha;
    }
}
