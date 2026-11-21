using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Despachos;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class DespachoRepository : Repository<Despacho>, IDespachoRepository
{
    public DespachoRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Despacho?> ObtenerPorPedidoAsync(int pedidoId)
        => await DbSet.FirstOrDefaultAsync(d => d.PedidoId == pedidoId);
}
