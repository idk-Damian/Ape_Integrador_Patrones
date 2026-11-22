using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

// recorre en profundidad los SKU finales de un kit multiplicando cantidades
// solo se marca la ruta actual, asi un mismo subkit puede aparecer en dos ramas sin ser ciclo
public sealed class IteradorComponentesFisicos : IEnumerator<ComponenteFisico>
{
    private readonly KitCompuesto _raiz;
    private readonly int _cantidadInicial;
    private readonly Stack<Marco> _pila = new();
    private readonly HashSet<string> _rutaActual = new();
    private ComponenteFisico? _actual;

    internal IteradorComponentesFisicos(KitCompuesto raiz, int cantidadInicial)
    {
        if (raiz is null) throw new DomainException("El kit no puede ser nulo.");
        if (cantidadInicial <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");
        _raiz = raiz;
        _cantidadInicial = cantidadInicial;
        Reset();
    }

    public ComponenteFisico Current => _actual ?? throw new InvalidOperationException("No hay elemento actual.");
    object System.Collections.IEnumerator.Current => Current;

    public bool MoveNext()
    {
        while (_pila.Count > 0)
        {
            var marco = _pila.Peek();
            if (marco.Indice >= marco.Kit.Componentes.Count)
            {
                _pila.Pop();
                _rutaActual.Remove(marco.Kit.Codigo);
                continue;
            }

            var componente = marco.Kit.Componentes[marco.Indice++];
            int cantidad = marco.Multiplicador * componente.Cantidad;

            if (componente.Elemento is KitCompuesto subKit)
            {
                Entrar(subKit, cantidad);
                continue;
            }

            var producto = (ProductoSimple)componente.Elemento;
            _actual = new ComponenteFisico(producto.Codigo, producto.Nombre, cantidad, producto.PrecioUnitario, producto.PesoKg);
            return true;
        }

        _actual = null;
        return false;
    }

    public void Reset()
    {
        _pila.Clear();
        _rutaActual.Clear();
        _actual = null;
        Entrar(_raiz, _cantidadInicial);
    }

    public void Dispose() { }

    private void Entrar(KitCompuesto kit, int multiplicador)
    {
        if (!_rutaActual.Add(kit.Codigo))
            throw new DomainException($"Ciclo detectado al recorrer el kit '{kit.Codigo}'.");
        _pila.Push(new Marco(kit, multiplicador));
    }

    private sealed class Marco
    {
        public Marco(KitCompuesto kit, int multiplicador)
        {
            Kit = kit;
            Multiplicador = multiplicador;
        }

        public KitCompuesto Kit { get; }
        public int Multiplicador { get; }
        public int Indice { get; set; }
    }
}
