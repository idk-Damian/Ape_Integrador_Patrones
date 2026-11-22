using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Devoluciones;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class DevolucionRepository : IDevolucionRepository
{
    private readonly LogiMarketDbContext _context;

    public DevolucionRepository(LogiMarketDbContext context) => _context = context;

    public Task<Devolucion?> ObtenerPorPedidoAsync(int pedidoId)
        => _context.Devoluciones.FirstOrDefaultAsync(d => d.PedidoId == pedidoId);

    public async Task AgregarAsync(Devolucion devolucion) => await _context.Devoluciones.AddAsync(devolucion);
}
