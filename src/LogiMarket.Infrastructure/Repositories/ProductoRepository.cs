using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class ProductoRepository : Repository<ProductoSimple>, IProductoRepository
{
    public ProductoRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<ProductoSimple?> ObtenerPorCodigoAsync(string codigo)
        => await DbSet.FirstOrDefaultAsync(p => p.Codigo == codigo);
}
