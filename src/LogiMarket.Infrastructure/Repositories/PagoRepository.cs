using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Pagos;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Pago?> ObtenerPorPedidoAsync(int pedidoId)
        => await DbSet.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

    public async Task<bool> ExisteClaveIdempotenciaAsync(string claveIdempotencia)
        => await DbSet.AnyAsync(p => p.ClaveIdempotencia == claveIdempotencia);
}
