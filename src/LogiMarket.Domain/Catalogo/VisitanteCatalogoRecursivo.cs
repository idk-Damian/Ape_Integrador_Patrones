using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

// base comun de los visitantes: baja por cada hijo con Aceptar y lleva el multiplicador
public abstract class VisitanteCatalogoRecursivo : IVisitanteCatalogo
{
    private readonly Stack<int> _multiplicadores = new();
    private readonly HashSet<string> _ruta = new();
    private readonly List<string> _kitsVisitados = new();

    protected VisitanteCatalogoRecursivo(int cantidadRaiz)
    {
        if (cantidadRaiz <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");
        _multiplicadores.Push(cantidadRaiz);
    }

    public IReadOnlyList<string> KitsVisitados => _kitsVisitados;

    public void VisitarProducto(ProductoSimple producto) => Acumular(producto, _multiplicadores.Peek());

    public void VisitarKit(KitCompuesto kit)
    {
        if (!_ruta.Add(kit.Codigo))
            throw new DomainException($"Ciclo detectado al visitar el kit '{kit.Codigo}'.");

        _kitsVisitados.Add(kit.Codigo);
        foreach (var componente in kit.Componentes)
        {
            _multiplicadores.Push(_multiplicadores.Peek() * componente.Cantidad);
            try
            {
                componente.Elemento.Aceptar(this);
            }
            finally
            {
                _multiplicadores.Pop();
            }
        }
        _ruta.Remove(kit.Codigo);
    }

    protected abstract void Acumular(ProductoSimple producto, int cantidad);
}
