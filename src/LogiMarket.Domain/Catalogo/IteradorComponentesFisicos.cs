using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón ITERATOR – Iterador propio que recorre los componentes físicos de un kit anidado,
/// propagando cantidades y evitando ciclos infinitos mediante un conjunto de visitados.
/// No expone la colección interna del kit.
/// </summary>
public sealed class IteradorComponentesFisicos : IEnumerable<ComponenteFisico>, IEnumerator<ComponenteFisico>
{
    private readonly Stack<FrameIteracion> _pila = new();
    private readonly HashSet<string> _visitados = new();
    private ComponenteFisico? _actual;

    public IteradorComponentesFisicos(KitCompuesto kit, int cantidadInicial = 1)
    {
        if (kit is null) throw new DomainException("El kit no puede ser nulo.");
        _visitados.Add(kit.Codigo);
        _pila.Push(new FrameIteracion(kit.Componentes, 0, cantidadInicial));
    }

    public ComponenteFisico Current
        => _actual ?? throw new InvalidOperationException("No hay elemento actual.");

    object System.Collections.IEnumerator.Current => Current;

    public bool MoveNext()
    {
        while (_pila.Count > 0)
        {
            var frame = _pila.Peek();

            if (frame.Indice >= frame.Componentes.Count)
            {
                _pila.Pop();
                continue;
            }

            var componente = frame.Componentes[frame.Indice];
            frame.Indice++;

            int cantidadTotal = frame.CantidadPadre * componente.Cantidad;

            if (componente.Elemento is KitCompuesto subKit)
            {
                if (!_visitados.Add(subKit.Codigo))
                    throw new DomainException($"Ciclo infinito detectado al iterar el kit '{subKit.Codigo}'.");

                _pila.Push(new FrameIteracion(subKit.Componentes, 0, cantidadTotal));
            }
            else if (componente.Elemento is ProductoSimple producto)
            {
                _actual = new ComponenteFisico(
                    producto.Codigo,
                    producto.Nombre,
                    cantidadTotal,
                    producto.PrecioUnitario,
                    producto.PesoKg);
                return true;
            }
        }

        _actual = null;
        return false;
    }

    public void Reset()
    {
        throw new NotSupportedException("El iterador no soporta Reset.");
    }

    public void Dispose() { }

    public IEnumerator<ComponenteFisico> GetEnumerator() => this;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this;

    private sealed class FrameIteracion
    {
        public IReadOnlyList<ComponenteKit> Componentes { get; }
        public int Indice { get; set; }
        public int CantidadPadre { get; }

        public FrameIteracion(IReadOnlyList<ComponenteKit> componentes, int indice, int cantidadPadre)
        {
            Componentes = componentes;
            Indice = indice;
            CantidadPadre = cantidadPadre;
        }
    }
}
