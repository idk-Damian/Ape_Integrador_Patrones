using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Application.Interfaces;

public interface IProductoRepository : IRepository<ProductoSimple>
{
    Task<ProductoSimple?> ObtenerPorCodigoAsync(string codigo);
}
