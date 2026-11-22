using LogiMarket.Application.Comun;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Metricas;
using LogiMarket.Application.Notificaciones;
using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Eventos;

// avisa al cliente por el canal configurado; el canal concreto lo fabrica el creador elegido
public sealed class ObservadorNotificaciones : IObservadorEventos
{
    private readonly SelectorCanalNotificacion _selector;
    private readonly IClienteRepository _clientes;
    private readonly MetricasSesion _metricas;

    public ObservadorNotificaciones(SelectorCanalNotificacion selector, IClienteRepository clientes, MetricasSesion metricas)
    {
        _selector = selector;
        _clientes = clientes;
        _metricas = metricas;
    }

    public string Nombre => "Notificación al cliente";

    public async Task ManejarAsync(EventoNegocio evento)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(evento.Entero("ClienteId"));
        if (cliente is null) return;

        var (asunto, mensaje) = evento.Tipo switch
        {
            TiposEvento.PedidoPagado => ($"Pedido #{evento.Entero("Id")} pagado",
                $"Recibimos su pago de {evento.Decimal("Total"):N2} USD. Gracias por su compra."),
            TiposEvento.DespachoRegistrado => ($"Pedido #{evento.Entero("Id")} en camino",
                $"Su guía es {evento.Texto("NumeroGuia")}. Llegada estimada {DateTime.Parse(evento.Texto("FechaEstimada")):dd/MM/yyyy}."),
            TiposEvento.DevolucionAceptada => ($"Devolución del pedido #{evento.Entero("Id")} aceptada",
                $"Le reembolsamos {evento.Decimal("Reembolso"):N2} USD (sin costo de envío)."),
            _ => ("Aviso LogiMarket", evento.Tipo)
        };

        await _selector.Obtener().NotificarAsync(cliente, asunto, mensaje, evento.Id);
        _metricas.RegistrarNotificacion();
    }
}

// deja una fila de auditoria de negocio por evento, una sola vez aunque el evento se reintente
public sealed class ObservadorAuditoria : IObservadorEventos
{
    private readonly IAuditoriaRepository _auditoria;

    public ObservadorAuditoria(IAuditoriaRepository auditoria) => _auditoria = auditoria;

    public string Nombre => "Auditoría de negocio";

    public async Task ManejarAsync(EventoNegocio evento)
    {
        var referencia = evento.Id.ToString("N");
        if (await _auditoria.ExisteAsync(CategoriasBitacora.Negocio, referencia)) return;
        await _auditoria.RegistrarAsync(new Auditoria(CategoriasBitacora.Negocio, evento.Tipo, referencia, "sistema",
            evento.Fecha, evento.Payload));
    }
}

public sealed class ObservadorMetricas : IObservadorEventos
{
    private readonly MetricasSesion _metricas;

    public ObservadorMetricas(MetricasSesion metricas) => _metricas = metricas;

    public string Nombre => "Métricas de sesión";

    public Task ManejarAsync(EventoNegocio evento)
    {
        _metricas.RegistrarEvento(evento.Id, evento.Tipo);
        return Task.CompletedTask;
    }
}
