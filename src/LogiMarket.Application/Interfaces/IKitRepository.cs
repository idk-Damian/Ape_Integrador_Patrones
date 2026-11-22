using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Application.Interfaces;

// los kits se devuelven con todo su arbol cargado para poder calcular precio y expandir SKU
public interface IKitRepository
{
    Task<KitCompuesto?> ObtenerPorCodigoAsync(string codigo);
    Task<IReadOnlyList<KitCompuesto>> ListarAsync();
    Task<bool> ExisteCodigoAsync(string codigo);
    Task AgregarAsync(KitCompuesto kit);
}
