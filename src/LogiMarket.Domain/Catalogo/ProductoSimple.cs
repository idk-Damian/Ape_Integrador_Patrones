using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón COMPOSITE – Hoja: Producto simple (SKU físico).
/// Dominio rico: sin setters públicos, invariantes protegidas.
/// </summary>
public class ProductoSimple : IElementoCatalogo
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Stock { get; private set; }
    public decimal PesoKg { get; private set; }
    public uint Version { get; private set; }

    public bool EsKit => false;

    private ProductoSimple() { }

    public ProductoSimple(string codigo, string nombre, decimal precioUnitario, int stock, decimal pesoKg)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new DomainException("El código del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del producto es obligatorio.");
        if (precioUnitario < 0) throw new DomainException("El precio no puede ser negativo.");
        if (stock < 0) throw new DomainException("El stock no puede ser negativo.");
        if (pesoKg < 0) throw new DomainException("El peso no puede ser negativo.");

        Codigo = codigo;
        Nombre = nombre;
        PrecioUnitario = Moneda.Redondear(precioUnitario);
        Stock = stock;
        PesoKg = pesoKg;
        Version = 0;
    }

    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0) throw new DomainException("El precio no puede ser negativo.");
        PrecioUnitario = Moneda.Redondear(nuevoPrecio);
    }

    public void AgregarStock(int cantidad)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser positiva.");
        Stock += cantidad;
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser positiva.");
        if (cantidad > Stock) throw new DomainException($"Stock insuficiente para '{Codigo}'. Disponible: {Stock}, solicitado: {cantidad}.");
        Stock -= cantidad;
        Version++;
    }

    public IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1)
    {
        yield return new ComponenteFisico(Codigo, Nombre, cantidadPadre, PrecioUnitario, PesoKg);
    }

    public void Aceptar(IVisitanteCatalogo visitante) => visitante.VisitarProducto(this);
}
