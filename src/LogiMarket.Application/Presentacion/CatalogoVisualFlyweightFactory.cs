namespace LogiMarket.Application.Presentacion;

// estado intrinseco compartido: lo que no cambia entre filas del mismo SKU
public sealed class MetadatosSku
{
    public MetadatosSku(string codigo, string nombre, string categoria, string descripcion)
    {
        Codigo = codigo;
        Nombre = nombre;
        Categoria = categoria;
        Descripcion = descripcion;
        Etiqueta = $"{codigo} - {nombre} ({categoria}) {descripcion}".Trim();
    }

    public string Codigo { get; }
    public string Nombre { get; }
    public string Categoria { get; }
    public string Descripcion { get; }
    public string Etiqueta { get; }
}

// estado extrinseco: stock, cantidad y precio cambian en cada renglon
public sealed class FilaCatalogoVisual
{
    public FilaCatalogoVisual(MetadatosSku metadatos, int stock, int cantidad, decimal precioVenta)
    {
        Metadatos = metadatos;
        Stock = stock;
        Cantidad = cantidad;
        PrecioVenta = precioVenta;
    }

    public MetadatosSku Metadatos { get; }
    public int Stock { get; }
    public int Cantidad { get; }
    public decimal PrecioVenta { get; }
}

// fabrica flyweight: entrega siempre la misma instancia de metadatos para un SKU
public sealed class CatalogoVisualFlyweightFactory
{
    private readonly Dictionary<string, MetadatosSku> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _candado = new();

    public int InstanciasCreadas { get { lock (_candado) return _cache.Count; } }
    public int Solicitudes { get; private set; }

    public MetadatosSku Obtener(string codigo, string nombre, string categoria, string descripcion)
    {
        lock (_candado)
        {
            Solicitudes++;
            if (!_cache.TryGetValue(codigo, out var metadatos))
            {
                metadatos = new MetadatosSku(codigo, nombre, categoria, descripcion);
                _cache[codigo] = metadatos;
            }
            return metadatos;
        }
    }

    public void Limpiar()
    {
        lock (_candado)
        {
            _cache.Clear();
            Solicitudes = 0;
        }
    }
}

public sealed record OrigenFila(string Codigo, string Nombre, string Categoria, string Descripcion, int Stock, decimal Precio);

public sealed record MedicionFlyweight(int Filas, int ObjetosMetadatosConFlyweight, int ObjetosMetadatosSinFlyweight,
    long BytesConFlyweight, long BytesSinFlyweight, bool MismaReferencia)
{
    public double Ahorro => BytesSinFlyweight == 0 ? 0 : 1 - (double)BytesConFlyweight / BytesSinFlyweight;
}

// compara las dos formas de armar las mismas filas, con los mismos datos y en el mismo hilo
public static class ComparativaFlyweight
{
    public static (List<FilaCatalogoVisual> Filas, MedicionFlyweight Medicion) Medir(IReadOnlyList<OrigenFila> origen, int filas)
    {
        if (origen.Count == 0) throw new ArgumentException("Se necesita al menos un SKU para medir.");

        var factory = new CatalogoVisualFlyweightFactory();
        // calentamiento para no medir la carga inicial del JIT
        ConFlyweight(origen, 10, new CatalogoVisualFlyweightFactory());
        SinFlyweight(origen, 10);

        GC.Collect();
        long antes = GC.GetAllocatedBytesForCurrentThread();
        var compartidas = ConFlyweight(origen, filas, factory);
        long bytesCon = GC.GetAllocatedBytesForCurrentThread() - antes;

        GC.Collect();
        antes = GC.GetAllocatedBytesForCurrentThread();
        var independientes = SinFlyweight(origen, filas);
        long bytesSin = GC.GetAllocatedBytesForCurrentThread() - antes;

        var primero = compartidas[0].Metadatos;
        bool misma = compartidas.Where(f => f.Metadatos.Codigo == primero.Codigo).All(f => ReferenceEquals(f.Metadatos, primero));
        int distintasSin = independientes.Select(f => f.Metadatos).Distinct(ReferenceEqualityComparer.Instance).Count();

        return (compartidas, new MedicionFlyweight(filas, factory.InstanciasCreadas, distintasSin, bytesCon, bytesSin, misma));
    }

    private static List<FilaCatalogoVisual> ConFlyweight(IReadOnlyList<OrigenFila> origen, int filas, CatalogoVisualFlyweightFactory factory)
    {
        var lista = new List<FilaCatalogoVisual>(filas);
        for (int i = 0; i < filas; i++)
        {
            var o = origen[i % origen.Count];
            lista.Add(new FilaCatalogoVisual(factory.Obtener(o.Codigo, o.Nombre, o.Categoria, o.Descripcion), o.Stock, i % 7 + 1, o.Precio));
        }
        return lista;
    }

    private static List<FilaCatalogoVisual> SinFlyweight(IReadOnlyList<OrigenFila> origen, int filas)
    {
        var lista = new List<FilaCatalogoVisual>(filas);
        for (int i = 0; i < filas; i++)
        {
            var o = origen[i % origen.Count];
            lista.Add(new FilaCatalogoVisual(new MetadatosSku(o.Codigo, o.Nombre, o.Categoria, o.Descripcion), o.Stock, i % 7 + 1, o.Precio));
        }
        return lista;
    }
}
