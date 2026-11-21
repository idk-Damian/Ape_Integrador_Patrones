using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Devoluciones;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class DevolucionRepository : Repository<Devolucion>, IDevolucionRepository
{
    public DevolucionRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Devolucion?> ObtenerPorPedidoAsync(int pedidoId)
        => await DbSet.FirstOrDefaultAsync(d => d.PedidoId == pedidoId);

    public async Task<bool> ExisteDevolucionParaPedidoAsync(int pedidoId)
        => await DbSet.AnyAsync(d => d.PedidoId == pedidoId);
}
