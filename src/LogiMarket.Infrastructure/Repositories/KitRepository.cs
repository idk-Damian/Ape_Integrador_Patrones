using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class KitRepository : Repository<KitCompuesto>, IKitRepository
{
    public KitRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<KitCompuesto?> ObtenerPorCodigoAsync(string codigo)
        => await DbSet
            .Include(k => k.Componentes)
            .FirstOrDefaultAsync(k => k.Codigo == codigo);

    public async Task<KitCompuesto?> ObtenerConComponentesAsync(int id)
        => await DbSet
            .Include(k => k.Componentes)
            .FirstOrDefaultAsync(k => k.Id == id);
}
