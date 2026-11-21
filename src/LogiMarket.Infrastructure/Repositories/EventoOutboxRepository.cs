using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Outbox;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class EventoOutboxRepository : Repository<EventoOutbox>, IEventoOutboxRepository
{
    public EventoOutboxRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync()
        => await DbSet.Where(e => !e.Procesado).OrderBy(e => e.FechaCreacion).ToListAsync();
}
