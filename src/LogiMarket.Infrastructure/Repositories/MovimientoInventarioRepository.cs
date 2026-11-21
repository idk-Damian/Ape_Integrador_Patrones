using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Inventario;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class MovimientoInventarioRepository : Repository<MovimientoInventario>, IMovimientoInventarioRepository
{
    public MovimientoInventarioRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MovimientoInventario>> ObtenerPorProductoAsync(int productoId)
        => await DbSet.Where(m => m.ProductoId == productoId).OrderByDescending(m => m.Fecha).ToListAsync();
}
