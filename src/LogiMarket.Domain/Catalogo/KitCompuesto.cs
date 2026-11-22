using LogiMarket.Domain.Common;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Catalogo;

// compuesto: un kit agrupa productos u otros kits
public class KitCompuesto : IElementoCatalogo
{
    private readonly List<ComponenteKit> _componentes = new();

    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public bool EsKit => true;

    public IReadOnlyList<ComponenteKit> Componentes => _componentes.AsReadOnly();

    public decimal PrecioUnitario
    {
        get
        {
            decimal total = 0m;
            foreach (var c in _componentes)
                total += c.Elemento.PrecioUnitario * c.Cantidad;
            return Moneda.Redondear(total);
        }
    }

    private KitCompuesto() { }

    public KitCompuesto(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new DomainException("El código del kit es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del kit es obligatorio.");

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
    }

    public void AgregarComponente(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new DomainException("El elemento no puede ser nulo.");

        if (elemento.Codigo == Codigo)
            throw new DomainException($"Referencia circular directa: el kit '{Codigo}' no puede contenerse a sí mismo.");

        if (elemento is KitCompuesto subKit && subKit.ContieneReferencia(Codigo))
            throw new DomainException(
                $"Referencia circular indirecta: '{elemento.Codigo}' ya contiene a '{Codigo}'.");

        if (_componentes.Exists(c => c.CodigoComponente == elemento.Codigo))
            throw new DomainException($"El componente '{elemento.Codigo}' ya existe en el kit '{Codigo}'.");

        _componentes.Add(new ComponenteKit(elemento, cantidad, _componentes.Count + 1));
    }

    internal bool ContieneReferencia(string codigo, HashSet<string>? revisados = null)
    {
        revisados ??= new HashSet<string>();
        if (!revisados.Add(Codigo)) return false;

        foreach (var componente in _componentes)
        {
            if (componente.CodigoComponente == codigo) return true;
            if (componente.Elemento is KitCompuesto subKit && subKit.ContieneReferencia(codigo, revisados))
                return true;
        }
        return false;
    }

    public IteradorComponentesFisicos CrearIterador(int cantidad = 1) => new(this, cantidad);

    // la expansion reutiliza el iterador para no tener dos recorridos distintos
    public IEnumerable<ComponenteFisico> ObtenerComponentesFisicos(int cantidadPadre = 1)
    {
        using var iterador = CrearIterador(cantidadPadre);
        while (iterador.MoveNext())
            yield return iterador.Current;
    }

    public void Aceptar(IVisitanteCatalogo visitante) => visitante.VisitarKit(this);
}
