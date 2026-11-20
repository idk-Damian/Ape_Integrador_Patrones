using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Interfaces;

public interface IEventoOutboxRepository : IRepository<EventoOutbox>
{
    Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync();
}
