using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class PedidoRepository : IPedidoRepository
{
    private readonly LogiMarketDbContext _context;

    public PedidoRepository(LogiMarketDbContext context) => _context = context;

    public Task<Pedido?> ObtenerPorIdAsync(int id)
        => ConDetalle().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Pedido>> ListarAsync()
        => await ConDetalle().OrderByDescending(p => p.Id).ToListAsync();

    public async Task AgregarAsync(Pedido pedido) => await _context.Pedidos.AddAsync(pedido);

    private IQueryable<Pedido> ConDetalle()
        => _context.Pedidos
            .Include(p => p.Lineas.OrderBy(l => l.Orden)).ThenInclude(l => l.Componentes.OrderBy(c => c.Orden))
            .Include(p => p.CambiosEstado.OrderBy(c => c.Secuencia))
            .AsSplitQuery();
}
