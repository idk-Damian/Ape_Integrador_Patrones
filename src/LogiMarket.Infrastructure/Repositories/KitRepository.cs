using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Infrastructure.Repositories;

// carga todos los kits y los productos que usan para que EF enlace el arbol completo;
// asi un kit anidado sale de la base listo para calcular precio y expandir SKU
public sealed class KitRepository : IKitRepository
{
    private readonly LogiMarketDbContext _context;

    public KitRepository(LogiMarketDbContext context) => _context = context;

    public async Task<KitCompuesto?> ObtenerPorCodigoAsync(string codigo)
    {
        var buscado = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        var kits = await CargarArbolAsync();
        return kits.FirstOrDefault(k => k.Codigo == buscado);
    }

    public async Task<IReadOnlyList<KitCompuesto>> ListarAsync()
        => (await CargarArbolAsync()).OrderBy(k => k.Codigo).ToList();

    public Task<bool> ExisteCodigoAsync(string codigo)
        => _context.Kits.AnyAsync(k => k.Codigo == (codigo ?? string.Empty).Trim().ToUpper());

    public async Task AgregarAsync(KitCompuesto kit) => await _context.Kits.AddAsync(kit);

    private async Task<List<KitCompuesto>> CargarArbolAsync()
    {
        var kits = await _context.Kits.Include(k => k.Componentes.OrderBy(c => c.Orden)).ToListAsync();

        var idsProductos = kits.SelectMany(k => k.Componentes)
            .Where(c => c.ProductoId.HasValue)
            .Select(c => c.ProductoId!.Value)
            .Distinct()
            .ToList();

        await _context.Productos.Where(p => idsProductos.Contains(p.Id)).LoadAsync();
        return kits;
    }
}
