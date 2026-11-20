using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Patrón COMPOSITE – Compuesto: Kit que contiene productos simples u otros kits.
/// Soporta anidamiento con detección/bloqueo de referencias circulares.
/// </summary>
public class KitCompuesto : IElementoCatalogo
{
    private readonly List<ComponenteKit> _componentes = new();

    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public bool EsKit => true;

    public IReadOnlyList<ComponenteKit> Componentes => _componentes.AsReadOnly();

    /// <summary>Precio del kit = suma recursiva de (precio componente × cantidad).</summary>
    public decimal PrecioUnitario
    {
        get
        {
            decimal total = 0m;
            foreach (var c in _componentes)
                total += Moneda.Redondear(c.Elemento.PrecioUnitario * c.Cantidad);
            return Moneda.Redondear(total);
        }
    }

    private KitCompuesto() { }

    public KitCompuesto(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new DomainException("El código del kit es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del kit es obligatorio.");

        Codigo = codigo;
        Nombre = nombre;
    }

    /// <summary>
    /// Agrega un componente al kit con detección de circularidad directa e indirecta.
    /// </summary>
    public void AgregarComponente(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new DomainException("El elemento no puede ser nulo.");
        if (cantidad <= 0) throw new DomainException("La cantidad debe ser mayor a cero.");

        if (elemento.Codigo == Codigo)
            throw new DomainException($"Referencia circular directa detectada: el kit '{Codigo}' no puede contenerse a sí mismo.");

        if (elemento is KitCompuesto subKit && subKit.ContieneReferencia(Codigo))
            throw new DomainException(
                $"Referencia circular indirecta detectada: '{elemento.Codigo}' ya contiene una referencia a '{Codigo}'.");

        var existente = _componentes.Find(c => c.CodigoComponente == elemento.Codigo);
        if (existente is not null)
            throw new DomainException($"El componente '{elemento.Codigo}' ya existe en el kit '{Codigo}'.");

        _componentes.Add(new ComponenteKit(elemento, cantidad));
    }

    /// <summary>
    /// Verifica recursivamente si este kit contiene una referencia (directa o indirecta) al código dado.
    /// Usa un conjunto de visitados para evitar ciclos infinitos.
    /// </summary>
    internal bool ContieneReferencia(string codigo, HashSet<string>? visitados = null)
    {
        visitados ??= new HashSet<string>();
        if (!visitados.Add(Codigo)) return false;

        foreach (var componente in _componentes)
        {
            if (componente.CodigoComponente == codigo) return true;
            if (componente.Elemento is KitCompuesto subKit && subKit.ContieneReferencia(codigo, visitados))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Patrón COMPOSITE – Expansión recursiva a SKU finales con cantidades propagadas.
    /// </summary>
    public IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1)
    {
        var visitados = new HashSet<string> { Codigo };
        return ObtenerComponentesFisicosInterno(cantidadPadre, visitados);
    }

    private IEnumerable<ComponenteFisico> ObtenerComponentesFisicosInterno(int cantidadPadre, HashSet<string> visitados)
    {
        foreach (var componente in _componentes)
        {
            int cantidadTotal = cantidadPadre * componente.Cantidad;

            if (componente.Elemento is KitCompuesto subKit)
            {
                if (!visitados.Add(subKit.Codigo))
                    throw new DomainException($"Ciclo detectado al expandir el kit '{subKit.Codigo}'.");

                foreach (var fisico in subKit.ObtenerComponentesFisicosInterno(cantidadTotal, visitados))
                    yield return fisico;

                visitados.Remove(subKit.Codigo);
            }
            else
            {
                foreach (var fisico in componente.Elemento.ObtenerComponentesFisicos(cantidadTotal))
                    yield return fisico;
            }
        }
    }

    public void Aceptar(IVisitanteCatalogo visitante) => visitante.VisitarKit(this);

    /// <summary>Restaura la lista de componentes desde persistencia.</summary>
    internal void CargarComponentes(IEnumerable<ComponenteKit> componentes)
    {
        _componentes.Clear();
        _componentes.AddRange(componentes);
    }

    /// <summary>Resuelve los elementos de catálogo referenciados por sus componentes.</summary>
    public void ResolverComponentes(Func<string, IElementoCatalogo?> buscador)
    {
        foreach (var comp in _componentes)
        {
            var elemento = buscador(comp.CodigoComponente)
                ?? throw new DomainException($"No se pudo resolver el elemento con código '{comp.CodigoComponente}'.");
            comp.ResolverElemento(elemento);
        }
    }
}
