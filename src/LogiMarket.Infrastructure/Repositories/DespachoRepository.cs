using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Despachos;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class DespachoRepository : IDespachoRepository
{
    private readonly LogiMarketDbContext _context;

    public DespachoRepository(LogiMarketDbContext context) => _context = context;

    public Task<Despacho?> ObtenerPorPedidoAsync(int pedidoId)
        => _context.Despachos.FirstOrDefaultAsync(d => d.PedidoId == pedidoId);

    public async Task AgregarAsync(Despacho despacho) => await _context.Despachos.AddAsync(despacho);
}
