using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

/// <summary>
/// Patrón STRATEGY – Selector de descuento.
/// Ordena las estrategias por prioridad descendente y retorna la primera que aplica.
/// Extensible: basta con agregar nuevas IEstrategiaDescuento sin editar este selector.
/// </summary>
public sealed class SelectorDescuento
{
    private readonly IReadOnlyList<IEstrategiaDescuento> _estrategias;

    public SelectorDescuento(IEnumerable<IEstrategiaDescuento> estrategias)
    {
        _estrategias = estrategias
            .OrderByDescending(e => e.Prioridad)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>Selecciona la estrategia con mayor prioridad que aplique al contexto.</summary>
    public IEstrategiaDescuento Seleccionar(int totalUnidadesFisicas, CategoriaCliente categoriaCliente)
    {
        foreach (var estrategia in _estrategias)
        {
            if (estrategia.Aplica(totalUnidadesFisicas, categoriaCliente))
                return estrategia;
        }
        return new SinDescuento();
    }

    /// <summary>Crea un selector con las tres estrategias estándar registradas.</summary>
    public static SelectorDescuento CrearEstandar()
        => new(new IEstrategiaDescuento[]
        {
            new Mayorista15(),
            new Vip10(),
            new SinDescuento()
        });
}
