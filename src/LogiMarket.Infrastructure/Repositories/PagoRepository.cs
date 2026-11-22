using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Pagos;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class PagoRepository : IPagoRepository
{
    private readonly LogiMarketDbContext _context;

    public PagoRepository(LogiMarketDbContext context) => _context = context;

    public Task<Pago?> ObtenerPorClaveIdempotenciaAsync(string clave)
        => _context.Pagos.FirstOrDefaultAsync(p => p.ClaveIdempotencia == clave.Trim());

    public Task<Pago?> ObtenerPorPedidoAsync(int pedidoId)
        => _context.Pagos.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

    public async Task AgregarAsync(Pago pago) => await _context.Pagos.AddAsync(pago);
}
