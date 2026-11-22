using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

// hoja del composite: un SKU fisico con existencias
public class ProductoSimple : IElementoCatalogo
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string Categoria { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Stock { get; private set; }
    public decimal PesoKg { get; private set; }
    public int Version { get; private set; }

    public bool EsKit => false;

    private ProductoSimple() { }

    public ProductoSimple(string codigo, string nombre, decimal precioUnitario, int stock, decimal pesoKg,
        string categoria = "Accesorios", string descripcion = "")
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new DomainException("El SKU del producto es obligatorio.");
        if (stock < 0) throw new DomainException("El stock no puede ser negativo.");

        Codigo = codigo.Trim().ToUpperInvariant();
        Stock = stock;
        ActualizarDatos(nombre, categoria, descripcion, pesoKg);
        ActualizarPrecio(precioUnitario);
        Version = 0;
    }

    public void ActualizarDatos(string nombre, string categoria, string descripcion, decimal pesoKg)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(categoria)) throw new DomainException("La categoría del producto es obligatoria.");
        if (pesoKg < 0) throw new DomainException("El peso no puede ser negativo.");

        Nombre = nombre.Trim();
        Categoria = categoria.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        PesoKg = pesoKg;
    }

    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0) throw new DomainException("El precio debe ser mayor a cero.");
        PrecioUnitario = Moneda.Redondear(nuevoPrecio);
    }

    public void AgregarStock(int cantidad)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser positiva.");
        Stock += cantidad;
        Version++;
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser positiva.");
        if (cantidad > Stock)
            throw new DomainException($"Stock insuficiente para '{Codigo}'. Disponible: {Stock}, solicitado: {cantidad}.");
        Stock -= cantidad;
        Version++;
    }

    public IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1)
    {
        if (cantidadPadre <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");
        yield return new ComponenteFisico(Codigo, Nombre, cantidadPadre, PrecioUnitario, PesoKg);
    }

    public void Aceptar(IVisitanteCatalogo visitante) => visitante.VisitarProducto(this);
}
