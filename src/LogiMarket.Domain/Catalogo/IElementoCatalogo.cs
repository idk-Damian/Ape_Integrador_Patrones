namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón COMPOSITE – Abstracción común para productos simples y kits compuestos.
/// </summary>
public interface IElementoCatalogo
{
    string Codigo { get; }
    string Nombre { get; }
    decimal PrecioUnitario { get; }
    bool EsKit { get; }

    /// <summary>
    /// Devuelve los componentes físicos (SKU finales) expandidos recursivamente,
    /// multiplicando cantidades por <paramref name="cantidadPadre"/>.
    /// </summary>
    IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1);

    /// <summary>Patrón VISITOR – Acepta un visitante para doble despacho.</summary>
    void Aceptar(IVisitanteCatalogo visitante);
}
