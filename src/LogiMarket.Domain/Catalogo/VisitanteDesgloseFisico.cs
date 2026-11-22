namespace LogiMarket.Domain.Catalogo;

// arma el desglose por SKU sumando cantidades repetidas
public sealed class VisitanteDesgloseFisico : VisitanteCatalogoRecursivo
{
    private readonly List<ComponenteFisico> _lineas = new();

    public VisitanteDesgloseFisico(int cantidadRaiz = 1) : base(cantidadRaiz) { }

    public IReadOnlyList<ComponenteFisico> Lineas => _lineas;

    protected override void Acumular(ProductoSimple producto, int cantidad)
    {
        int indice = _lineas.FindIndex(l => l.Codigo == producto.Codigo);
        if (indice >= 0)
        {
            _lineas[indice] = _lineas[indice] with { Cantidad = _lineas[indice].Cantidad + cantidad };
            return;
        }
        _lineas.Add(new ComponenteFisico(producto.Codigo, producto.Nombre, cantidad, producto.PrecioUnitario, producto.PesoKg));
    }
}
