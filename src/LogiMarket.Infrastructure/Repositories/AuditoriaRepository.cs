using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Outbox;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class AuditoriaRepository : Repository<Auditoria>, IAuditoriaRepository
{
    public AuditoriaRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Auditoria>> ObtenerPorEntidadAsync(string entidad, int entidadId)
        => await DbSet.Where(a => a.Entidad == entidad && a.EntidadId == entidadId).OrderByDescending(a => a.Fecha).ToListAsync();
}
