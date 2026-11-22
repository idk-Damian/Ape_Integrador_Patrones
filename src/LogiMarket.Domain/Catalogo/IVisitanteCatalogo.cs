namespace LogiMarket.Domain.Catalogo;

public interface IVisitanteCatalogo
{
    void VisitarProducto(ProductoSimple producto);
    void VisitarKit(KitCompuesto kit);
}
