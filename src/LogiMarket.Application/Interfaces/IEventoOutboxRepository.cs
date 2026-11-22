using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Interfaces;

public interface IEventoOutboxRepository
{
    Task AgregarAsync(EventoOutbox evento);
    Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync(int maximo = 50);
    Task<IReadOnlyList<EventoOutbox>> ListarAsync();
}
