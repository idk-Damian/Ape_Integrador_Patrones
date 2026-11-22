using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Domain.Despachos;

public class Despacho
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public string NumeroGuia { get; private set; } = null!;
    public TipoEntrega TipoEntrega { get; private set; }
    public DateTime FechaDespacho { get; private set; }
    public DateTime FechaEstimada { get; private set; }
    public DateTime? FechaEntrega { get; private set; }

    private Despacho() { }

    // la guia y el plazo salen de la misma familia con la que se vendio el pedido
    public static Despacho Registrar(Pedido pedido, IFabricaEntrega fabrica, DateTime fecha)
    {
        if (pedido is null || pedido.Id <= 0) throw new DomainException("El pedido a despachar no es válido.");
        if (fabrica is null) throw new DomainException("Falta la forma de entrega.");
        if (fabrica.Tipo != pedido.TipoEntrega)
            throw new DomainException(
                $"El pedido se vendió con entrega {pedido.TipoEntrega} y no puede despacharse como {fabrica.Tipo}.");

        var plazo = fabrica.CrearPlazo();
        var etiqueta = fabrica.CrearEtiqueta();
        if (plazo.Familia != fabrica.Tipo || etiqueta.Familia != fabrica.Tipo)
            throw new DomainException("La fábrica de entrega mezcló productos de familias distintas.");

        return new Despacho
        {
            PedidoId = pedido.Id,
            NumeroGuia = etiqueta.GenerarNumeroGuia(pedido.Id, fecha),
            TipoEntrega = fabrica.Tipo,
            FechaDespacho = fecha,
            FechaEstimada = plazo.EstimarEntrega(fecha)
        };
    }

    public void RegistrarEntrega(DateTime fecha)
    {
        if (FechaEntrega is not null) throw new DomainException("El despacho ya fue entregado.");
        if (fecha < FechaDespacho) throw new DomainException("La entrega no puede ser anterior al despacho.");
        FechaEntrega = fecha;
    }
}
