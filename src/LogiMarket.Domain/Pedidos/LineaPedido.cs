using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Common;

namespace LogiMarket.Domain.Pedidos;

/// <summary>
/// Línea de un pedido. Almacena snapshot de precio/composición al momento de agregar.
/// </summary>
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
    public decimal Subtotal => Moneda.Redondear(PrecioUnitario * Cantidad);

    public IReadOnlyList<LineaComponenteSnapshot> Componentes => _componentes.AsReadOnly();

    /// <summary>Total de unidades físicas finales (expandidas) en esta línea.</summary>
    public int TotalUnidadesFisicas
    {
        get
        {
            if (!EsKit) return Cantidad;
            int total = 0;
            foreach (var c in _componentes) total += c.Cantidad * Cantidad;
            return total;
        }
    }

    private LineaPedido() { }

    /// <summary>Constructor interno para restaurar desde memento sin reflexión.</summary>
    internal LineaPedido(string elementoCodigo, string elementoNombre, decimal precioUnitario,
        int cantidad, bool esKit, IEnumerable<LineaComponenteSnapshot> componentes)
    {
        ElementoCodigo = elementoCodigo;
        ElementoNombre = elementoNombre;
        PrecioUnitario = precioUnitario;
        Cantidad = cantidad;
        EsKit = esKit;
        _componentes.AddRange(componentes);
    }

    public LineaPedido(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new Exceptions.DomainException("El elemento no puede ser nulo.");
        if (cantidad <= 0) throw new Exceptions.DomainException("La cantidad debe ser mayor a cero.");

        ElementoCodigo = elemento.Codigo;
        ElementoNombre = elemento.Nombre;
        PrecioUnitario = elemento.PrecioUnitario;
        Cantidad = cantidad;
        EsKit = elemento.EsKit;

        foreach (var componente in elemento.ObtenerComponentesFisicos(1))
        {
            _componentes.Add(new LineaComponenteSnapshot(
                componente.Codigo, componente.Nombre,
                componente.PrecioUnitario, componente.Cantidad, componente.PesoKg));
        }
    }

    internal LineaPedido ClonarProfundo()
    {
        return new LineaPedido(
            ElementoCodigo,
            ElementoNombre,
            PrecioUnitario,
            Cantidad,
            EsKit,
            _componentes.Select(c => c.ClonarProfundo()));
    }

    internal void CargarComponentes(IEnumerable<LineaComponenteSnapshot> componentes)
    {
        _componentes.Clear();
        _componentes.AddRange(componentes);
    }
}
