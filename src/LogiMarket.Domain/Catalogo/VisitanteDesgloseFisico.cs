namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón VISITOR – Genera un desglose físico por SKU agregando cantidades.
/// </summary>
public sealed class VisitanteDesgloseFisico : IVisitanteCatalogo
{
    private readonly Dictionary<string, ComponenteFisico> _desglose = new();
    private readonly int _cantidadRaiz;

    public IReadOnlyDictionary<string, ComponenteFisico> Desglose => _desglose;

    public VisitanteDesgloseFisico(int cantidadRaiz = 1)
    {
        _cantidadRaiz = cantidadRaiz;
    }

    public void VisitarProducto(ProductoSimple producto)
    {
        AgregarComponente(new ComponenteFisico(
            producto.Codigo, producto.Nombre, _cantidadRaiz,
            producto.PrecioUnitario, producto.PesoKg));
    }

    public void VisitarKit(KitCompuesto kit)
    {
        foreach (var componente in kit.ObtenerComponentesFisicos(_cantidadRaiz))
        {
            AgregarComponente(componente);
        }
    }

    private void AgregarComponente(ComponenteFisico componente)
    {
        if (_desglose.TryGetValue(componente.Codigo, out var existente))
        {
            _desglose[componente.Codigo] = existente with
            {
                Cantidad = existente.Cantidad + componente.Cantidad
            };
        }
        else
        {
            _desglose[componente.Codigo] = componente;
        }
    }
}
