using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Outbox;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class AuditoriaRepository : IAuditoriaRepository
{
    private readonly LogiMarketDbContext _context;

    public AuditoriaRepository(LogiMarketDbContext context) => _context = context;

    public async Task RegistrarAsync(Auditoria registro) => await _context.Auditorias.AddAsync(registro);

    public async Task<IReadOnlyList<Auditoria>> ListarAsync(string? categoria = null)
        => await _context.Auditorias
            .Where(a => categoria == null || a.Categoria == categoria)
            .OrderByDescending(a => a.Id)
            .ToListAsync();
}
