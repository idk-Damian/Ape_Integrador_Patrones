using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Pedido?> ObtenerConLineasAsync(int id)
        => await DbSet
            .Include(p => p.Lineas)
            .ThenInclude(l => l.Componentes)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Pedido>> ObtenerPorClienteAsync(int clienteId)
        => await DbSet
            .Include(p => p.Lineas)
            .ThenInclude(l => l.Componentes)
            .Where(p => p.ClienteId == clienteId)
            .ToListAsync();
}
