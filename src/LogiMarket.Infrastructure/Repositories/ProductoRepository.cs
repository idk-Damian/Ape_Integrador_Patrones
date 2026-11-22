using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class ProductoRepository : IProductoRepository
{
    private readonly LogiMarketDbContext _context;

    public ProductoRepository(LogiMarketDbContext context) => _context = context;

    public Task<ProductoSimple?> ObtenerPorCodigoAsync(string codigo)
        => _context.Productos.FirstOrDefaultAsync(p => p.Codigo == Normalizar(codigo));

    public async Task<IReadOnlyList<ProductoSimple>> ObtenerPorCodigosAsync(IEnumerable<string> codigos)
    {
        var lista = codigos.Select(Normalizar).Distinct().ToList();
        return await _context.Productos.Where(p => lista.Contains(p.Codigo)).ToListAsync();
    }

    public async Task<IReadOnlyList<ProductoSimple>> ListarAsync(string? texto = null)
    {
        var consulta = _context.Productos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim().ToUpper();
            consulta = consulta.Where(p => p.Codigo.Contains(filtro) || p.Nombre.ToUpper().Contains(filtro));
        }
        return await consulta.OrderBy(p => p.Codigo).ToListAsync();
    }

    public Task<bool> ExisteCodigoAsync(string codigo)
        => _context.Productos.AnyAsync(p => p.Codigo == Normalizar(codigo));

    public async Task AgregarAsync(ProductoSimple producto) => await _context.Productos.AddAsync(producto);

    private static string Normalizar(string codigo) => (codigo ?? string.Empty).Trim().ToUpperInvariant();
}
