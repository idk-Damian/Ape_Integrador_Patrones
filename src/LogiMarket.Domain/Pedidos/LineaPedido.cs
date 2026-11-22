using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos;

// guarda una foto del precio y de la composicion al momento de agregar la linea
public class LineaPedido
{
    private readonly List<LineaComponenteSnapshot> _componentes = new();

    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public string ElementoCodigo { get; private set; } = null!;
    public string ElementoNombre { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Cantidad { get; private set; }
    public bool EsKit { get; private set; }
    public int Orden { get; private set; }

    public decimal Subtotal => Moneda.Redondear(PrecioUnitario * Cantidad);
    public IReadOnlyList<LineaComponenteSnapshot> Componentes => _componentes.AsReadOnly();

    public int TotalUnidadesFisicas => _componentes.Sum(c => c.Cantidad) * Cantidad;

    private LineaPedido() { }

    internal LineaPedido(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new DomainException("El elemento no puede ser nulo.");
        ValidarCantidad(cantidad);

        ElementoCodigo = elemento.Codigo;
        ElementoNombre = elemento.Nombre;
        PrecioUnitario = elemento.PrecioUnitario;
        Cantidad = cantidad;
        EsKit = elemento.EsKit;

        foreach (var c in elemento.ObtenerComponentesFisicos(1))
            _componentes.Add(new LineaComponenteSnapshot(c.Codigo, c.Nombre, c.PrecioUnitario, c.Cantidad, c.PesoKg,
                _componentes.Count + 1));
    }

    private LineaPedido(string codigo, string nombre, decimal precio, int cantidad, bool esKit, int orden,
        IEnumerable<LineaComponenteSnapshot> componentes)
    {
        Orden = orden;
        ElementoCodigo = codigo;
        ElementoNombre = nombre;
        PrecioUnitario = precio;
        Cantidad = cantidad;
        EsKit = esKit;
        _componentes.AddRange(componentes);
    }

    internal void CambiarCantidad(int cantidad)
    {
        ValidarCantidad(cantidad);
        Cantidad = cantidad;
    }

    internal void AsignarOrden(int orden) => Orden = orden;

    // copia profunda: nuevas instancias de la linea y de cada componente
    internal LineaPedido Clonar()
        => new(ElementoCodigo, ElementoNombre, PrecioUnitario, Cantidad, EsKit, Orden, _componentes.Select(c => c.Clonar()));

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");
    }
}
