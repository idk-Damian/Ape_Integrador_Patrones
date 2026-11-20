using LogiMarket.Domain.Common;

namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón VISITOR – Calcula el peso logístico total de un elemento del catálogo.
/// Para KIT_TELETRABAJO (P01 0.10 + P02 0.60 + P03 0.30) debe dar exactamente 1.00 kg.
/// </summary>
public sealed class VisitantePesoLogistico : IVisitanteCatalogo
{
    private readonly int _cantidadRaiz;

    public decimal PesoTotal { get; private set; }

    public VisitantePesoLogistico(int cantidadRaiz = 1)
    {
        _cantidadRaiz = cantidadRaiz;
    }

    public void VisitarProducto(ProductoSimple producto)
    {
        PesoTotal += Moneda.Redondear(producto.PesoKg * _cantidadRaiz);
    }

    public void VisitarKit(KitCompuesto kit)
    {
        foreach (var componente in kit.ObtenerComponentesFisicos(_cantidadRaiz))
        {
            PesoTotal += Moneda.Redondear(componente.PesoKg * componente.Cantidad);
        }
    }
}
