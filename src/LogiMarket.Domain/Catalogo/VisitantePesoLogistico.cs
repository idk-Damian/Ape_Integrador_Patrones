namespace LogiMarket.Domain.Catalogo;

// suma el peso de cada SKU final por su cantidad
public sealed class VisitantePesoLogistico : VisitanteCatalogoRecursivo
{
    private decimal _acumulado;

    public VisitantePesoLogistico(int cantidadRaiz = 1) : base(cantidadRaiz) { }

    public decimal PesoTotalKg => Math.Round(_acumulado, 2, MidpointRounding.AwayFromZero);

    protected override void Acumular(ProductoSimple producto, int cantidad)
        => _acumulado += producto.PesoKg * cantidad;
}
