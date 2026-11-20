namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Componente de un kit: referencia a un elemento del catálogo con cantidad.
/// Almacena datos primitivos para persistencia y resuelve la referencia en tiempo de ejecución.
/// </summary>
public class ComponenteKit
{
    public int Id { get; private set; }
    public int KitPadreId { get; private set; }
    public string CodigoComponente { get; private set; } = null!;
    public string TipoComponente { get; private set; } = null!;
    public int Cantidad { get; private set; }

    private IElementoCatalogo? _elementoResuelto;

    public IElementoCatalogo Elemento
        => _elementoResuelto ?? throw new Exceptions.DomainException(
            $"Componente '{CodigoComponente}' no resuelto. Llame a ResolverElemento primero.");

    private ComponenteKit() { }

    public ComponenteKit(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new Exceptions.DomainException("El elemento no puede ser nulo.");
        if (cantidad <= 0) throw new Exceptions.DomainException("La cantidad debe ser mayor a cero.");

        CodigoComponente = elemento.Codigo;
        TipoComponente = elemento.EsKit ? "KIT" : "PRODUCTO";
        Cantidad = cantidad;
        _elementoResuelto = elemento;
    }

    /// <summary>
    /// Resuelve la referencia al elemento del catálogo (usado al rehidratar desde persistencia).
    /// </summary>
    public void ResolverElemento(IElementoCatalogo elemento)
    {
        if (elemento.Codigo != CodigoComponente)
            throw new Exceptions.DomainException(
                $"El código del elemento ({elemento.Codigo}) no coincide con el componente ({CodigoComponente}).");
        _elementoResuelto = elemento;
    }
}
