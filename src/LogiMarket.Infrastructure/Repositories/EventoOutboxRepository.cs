using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Outbox;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class EventoOutboxRepository : IEventoOutboxRepository
{
    private readonly LogiMarketDbContext _context;

    public EventoOutboxRepository(LogiMarketDbContext context) => _context = context;

    public async Task AgregarAsync(EventoOutbox evento) => await _context.EventosOutbox.AddAsync(evento);

    public Task<EventoOutbox?> ObtenerPorIdAsync(int id) => _context.EventosOutbox.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync(int maximo = 50)
        => await _context.EventosOutbox
            .Where(e => e.FechaProcesado == null)
            .OrderBy(e => e.Id)
            .Take(maximo)
            .ToListAsync();

    public async Task<IReadOnlyList<EventoOutbox>> ListarAsync()
        => await _context.EventosOutbox.OrderByDescending(e => e.Id).ToListAsync();
}
