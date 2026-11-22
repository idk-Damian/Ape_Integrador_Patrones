using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Inventario;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly LogiMarketDbContext _context;

    public MovimientoInventarioRepository(LogiMarketDbContext context) => _context = context;

    public async Task AgregarAsync(MovimientoInventario movimiento)
        => await _context.MovimientosInventario.AddAsync(movimiento);

    public async Task<IReadOnlyList<MovimientoInventario>> ListarPorPedidoAsync(int pedidoId)
        => await _context.MovimientosInventario.Where(m => m.PedidoId == pedidoId).OrderBy(m => m.Id).ToListAsync();

    public async Task<IReadOnlyList<MovimientoInventario>> ListarAsync(int? productoId = null)
        => await _context.MovimientosInventario
            .Where(m => productoId == null || m.ProductoId == productoId)
            .OrderByDescending(m => m.Id)
            .ToListAsync();
}
