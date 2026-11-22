using LogiMarket.Domain.Clientes;

namespace LogiMarket.Application.Notificaciones;

public interface INotificador
{
    string Canal { get; }
    Task EnviarAsync(Cliente destinatario, string asunto, string mensaje, Guid referencia);
}

// factory method: cada creador concreto decide que notificador fabricar; el resto del envio es comun
public abstract class CreadorNotificador
{
    public abstract string Canal { get; }

    public abstract INotificador CrearNotificador();

    public async Task<string> NotificarAsync(Cliente destinatario, string asunto, string mensaje, Guid referencia)
    {
        var notificador = CrearNotificador();
        await notificador.EnviarAsync(destinatario, asunto, mensaje, referencia);
        return notificador.Canal;
    }
}

public static class CanalesNotificacion
{
    public const string Email = "EMAIL";
    public const string Sms = "SMS";
}

// preferencia de canal de la sesion; se cambia desde el panel, no desde el formulario de pedidos
public sealed class ConfiguracionNotificaciones
{
    public string Canal { get; set; } = CanalesNotificacion.Email;
}

public sealed class SelectorCanalNotificacion
{
    private readonly IReadOnlyDictionary<string, CreadorNotificador> _creadores;
    private readonly ConfiguracionNotificaciones _configuracion;

    public SelectorCanalNotificacion(IEnumerable<CreadorNotificador> creadores, ConfiguracionNotificaciones configuracion)
    {
        _creadores = creadores.ToDictionary(c => c.Canal);
        _configuracion = configuracion;
    }

    public IReadOnlyCollection<string> Canales => _creadores.Keys.ToList();

    public CreadorNotificador Obtener()
        => _creadores.TryGetValue(_configuracion.Canal, out var creador)
            ? creador
            : throw new InvalidOperationException($"No hay creador registrado para el canal {_configuracion.Canal}.");
}
