using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Application.Interfaces;

public interface IProductoRepository
{
    Task<ProductoSimple?> ObtenerPorCodigoAsync(string codigo);
    Task<IReadOnlyList<ProductoSimple>> ObtenerPorCodigosAsync(IEnumerable<string> codigos);
    Task<IReadOnlyList<ProductoSimple>> ListarAsync(string? texto = null);
    Task<bool> ExisteCodigoAsync(string codigo);
    Task AgregarAsync(ProductoSimple producto);
}
