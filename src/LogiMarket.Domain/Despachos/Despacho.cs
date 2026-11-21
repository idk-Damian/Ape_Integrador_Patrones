using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Despachos;

public class Despacho
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public string Direccion { get; private set; } = null!;
    public DateTime Fecha { get; private set; }
    public DateTime? FechaEntrega { get; private set; }

    private Despacho() { }

    public Despacho(int pedidoId, string direccion)
    {
        if (pedidoId <= 0) throw new DomainException("ID de pedido inválido.");
        if (string.IsNullOrWhiteSpace(direccion)) throw new DomainException("La dirección es obligatoria.");

        PedidoId = pedidoId;
        Direccion = direccion;
        Fecha = DateTime.UtcNow;
    }

    public void MarcarEntregado()
    {
        FechaEntrega = DateTime.UtcNow;
    }
}
