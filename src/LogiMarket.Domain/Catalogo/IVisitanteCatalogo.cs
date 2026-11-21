namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón VISITOR – Interfaz de visitante para el catálogo.
/// </summary>
public interface IVisitanteCatalogo
{
    void VisitarProducto(ProductoSimple producto);
    void VisitarKit(KitCompuesto kit);
}
