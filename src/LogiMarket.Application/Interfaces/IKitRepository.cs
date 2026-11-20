using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Application.Interfaces;

public interface IKitRepository : IRepository<KitCompuesto>
{
    Task<KitCompuesto?> ObtenerPorCodigoAsync(string codigo);
    Task<KitCompuesto?> ObtenerConComponentesAsync(int id);
}
