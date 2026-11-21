namespace LogiMarket.Domain.Outbox;

public class EventoOutbox
{
    public int Id { get; private set; }
    public string TipoEvento { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime FechaCreacion { get; private set; }
    public bool Procesado { get; private set; }

    private EventoOutbox() { }

    public EventoOutbox(string tipoEvento, string payload)
    {
        TipoEvento = tipoEvento;
        Payload = payload;
        FechaCreacion = DateTime.UtcNow;
        Procesado = false;
    }

    public void MarcarProcesado() => Procesado = true;
}
