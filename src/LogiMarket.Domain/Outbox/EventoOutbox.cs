using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Outbox;

// evento pendiente que se guarda en la misma transaccion del negocio y se procesa despues
public class EventoOutbox
{
    public int Id { get; private set; }
    public Guid IdEvento { get; private set; }
    public string TipoEvento { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaProcesado { get; private set; }
    public int Intentos { get; private set; }
    public string? UltimoError { get; private set; }

    public bool Procesado => FechaProcesado.HasValue;

    private EventoOutbox() { }

    public EventoOutbox(string tipoEvento, string payload, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(tipoEvento)) throw new DomainException("El tipo de evento es obligatorio.");
        IdEvento = Guid.NewGuid();
        TipoEvento = tipoEvento;
        Payload = payload ?? string.Empty;
        FechaCreacion = fecha;
    }

    public void MarcarProcesado(DateTime fecha)
    {
        if (Procesado) throw new DomainException("El evento ya fue procesado.");
        Intentos++;
        UltimoError = null;
        FechaProcesado = fecha;
    }

    public void RegistrarFallo(string error)
    {
        if (Procesado) throw new DomainException("El evento ya fue procesado.");
        Intentos++;
        UltimoError = error.Length > 500 ? error[..500] : error;
    }
}
