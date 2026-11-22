using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

// renglon de un kit: apunta a un producto o a otro kit, nunca a ambos
public class ComponenteKit
{
    public int Id { get; private set; }
    public int KitPadreId { get; private set; }
    public int? ProductoId { get; private set; }
    public int? SubKitId { get; private set; }
    public int Cantidad { get; private set; }
    public int Orden { get; private set; }

    public ProductoSimple? Producto { get; private set; }
    public KitCompuesto? SubKit { get; private set; }

    public IElementoCatalogo Elemento
        => (IElementoCatalogo?)Producto ?? SubKit
           ?? throw new DomainException($"El componente {Id} del kit no fue cargado junto con su elemento.");

    public string CodigoComponente => Elemento.Codigo;

    private ComponenteKit() { }

    internal ComponenteKit(IElementoCatalogo elemento, int cantidad, int orden)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");

        switch (elemento)
        {
            case ProductoSimple producto:
                Producto = producto;
                break;
            case KitCompuesto kit:
                SubKit = kit;
                break;
            default:
                throw new DomainException("Tipo de elemento de catálogo no soportado.");
        }
        Cantidad = cantidad;
        Orden = orden;
    }
}
