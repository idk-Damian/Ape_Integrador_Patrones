using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Catalogo;

namespace LogiMarket.Application.Pedidos;

// busca un codigo primero como producto y luego como kit
public sealed class ResolutorCatalogo
{
    private readonly IProductoRepository _productos;
    private readonly IKitRepository _kits;

    public ResolutorCatalogo(IProductoRepository productos, IKitRepository kits)
    {
        _productos = productos;
        _kits = kits;
    }

    public async Task<IElementoCatalogo> ObtenerAsync(string codigo)
    {
        IElementoCatalogo? elemento = await _productos.ObtenerPorCodigoAsync(codigo);
        elemento ??= await _kits.ObtenerPorCodigoAsync(codigo);
        return elemento ?? throw new RecursoNoEncontradoException($"No existe el producto o kit '{codigo}'.");
    }
}
