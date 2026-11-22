using LogiMarket.Application.Consultas;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Consultas;

public sealed class ConsultasPedidos : IConsultasPedidos
{
    private readonly LogiMarketDbContext _context;

    public ConsultasPedidos(LogiMarketDbContext context) => _context = context;

    public async Task<IReadOnlyList<PedidoResumen>> ListarResumenAsync()
        => await (from p in _context.Pedidos.AsNoTracking()
                  join c in _context.Clientes.AsNoTracking() on p.ClienteId equals c.Id
                  orderby p.Id descending
                  select new PedidoResumen(p.Id, c.Nombre, p.EstadoNombre, p.Total, p.FechaCreacion, p.TipoEntrega))
            .ToListAsync();
}
