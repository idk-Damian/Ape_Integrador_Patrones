using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Descuentos;

// recibe las politicas desde afuera; agregar una nueva no obliga a tocar esta clase
public sealed class CalculadoraDescuento
{
    private static readonly IEstrategiaDescuento PorDefecto = new SinDescuento();
    private readonly IReadOnlyList<IEstrategiaDescuento> _estrategias;

    public CalculadoraDescuento(IEnumerable<IEstrategiaDescuento> estrategias)
    {
        if (estrategias is null) throw new DomainException("Debe indicar las políticas de descuento.");
        var lista = estrategias.ToList();

        if (lista.Select(e => e.Prioridad).Distinct().Count() != lista.Count)
            throw new DomainException("Dos políticas de descuento no pueden tener la misma prioridad.");

        _estrategias = lista.OrderByDescending(e => e.Prioridad).ToList();
    }

    public IReadOnlyList<IEstrategiaDescuento> Politicas => _estrategias;

    public IEstrategiaDescuento Seleccionar(ContextoDescuento contexto)
        => _estrategias.FirstOrDefault(e => e.Aplica(contexto)) ?? PorDefecto;

    public ResultadoDescuento Calcular(ContextoDescuento contexto)
    {
        var estrategia = Seleccionar(contexto);
        return new ResultadoDescuento(estrategia.Nombre, estrategia.CalcularDescuento(contexto.Subtotal));
    }
}
