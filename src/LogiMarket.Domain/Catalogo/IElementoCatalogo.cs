namespace LogiMarket.Domain.Catalogo;

// componente comun del composite (producto o kit)
public interface IElementoCatalogo
{
    string Codigo { get; }
    string Nombre { get; }
    decimal PrecioUnitario { get; }
    bool EsKit { get; }

    IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1);

    void Aceptar(IVisitanteCatalogo visitante);
}
