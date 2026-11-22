using System.Collections.Concurrent;
using LogiMarket.Application.Eventos;

namespace LogiMarket.Application.Metricas;

// unica instancia por aplicacion (registrada como singleton en DI); la base sigue siendo la fuente de verdad,
// esto solo cuenta lo ocurrido durante la sesion para el panel
public sealed class MetricasSesion
{
    private readonly ConcurrentDictionary<Guid, byte> _eventosVistos = new();
    private long _pedidosPagados;
    private long _despachos;
    private long _devoluciones;
    private long _notificaciones;
    private long _operacionesRechazadas;

    public event Action? Cambio;

    public DateTime Inicio { get; } = DateTime.Now;
    public long PedidosPagados => Interlocked.Read(ref _pedidosPagados);
    public long Despachos => Interlocked.Read(ref _despachos);
    public long Devoluciones => Interlocked.Read(ref _devoluciones);
    public long Notificaciones => Interlocked.Read(ref _notificaciones);
    public long OperacionesRechazadas => Interlocked.Read(ref _operacionesRechazadas);
    public int EventosProcesados => _eventosVistos.Count;

    // devuelve false si el evento ya se habia contado (evita sumar dos veces en un reintento)
    public bool RegistrarEvento(Guid idEvento, string tipo)
    {
        if (!_eventosVistos.TryAdd(idEvento, 0)) return false;
        switch (tipo)
        {
            case TiposEvento.PedidoPagado: Interlocked.Increment(ref _pedidosPagados); break;
            case TiposEvento.DespachoRegistrado: Interlocked.Increment(ref _despachos); break;
            case TiposEvento.DevolucionAceptada: Interlocked.Increment(ref _devoluciones); break;
        }
        Cambio?.Invoke();
        return true;
    }

    public void RegistrarNotificacion()
    {
        Interlocked.Increment(ref _notificaciones);
        Cambio?.Invoke();
    }

    public void RegistrarRechazo()
    {
        Interlocked.Increment(ref _operacionesRechazadas);
        Cambio?.Invoke();
    }
}
