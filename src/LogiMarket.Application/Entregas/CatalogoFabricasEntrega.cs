using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Entregas;

// entrega la fabrica concreta segun el tipo elegido, sin que el caso de uso conozca las clases
public sealed class CatalogoFabricasEntrega
{
    private readonly IReadOnlyDictionary<TipoEntrega, IFabricaEntrega> _fabricas;

    public CatalogoFabricasEntrega(IEnumerable<IFabricaEntrega> fabricas)
        => _fabricas = fabricas.ToDictionary(f => f.Tipo);

    public IFabricaEntrega Obtener(TipoEntrega tipo)
        => _fabricas.TryGetValue(tipo, out var fabrica)
            ? fabrica
            : throw new DomainException($"No hay una familia de entrega registrada para {tipo}.");

    public IReadOnlyCollection<TipoEntrega> Disponibles => _fabricas.Keys.ToList();
}
