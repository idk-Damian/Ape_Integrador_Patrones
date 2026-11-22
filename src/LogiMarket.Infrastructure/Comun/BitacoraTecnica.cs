using LogiMarket.Application.Comun;

namespace LogiMarket.Infrastructure.Comun;

// guarda en memoria para la pantalla de auditoria y, si hay carpeta, tambien en archivos .log por categoria
public sealed class BitacoraTecnica : IBitacora
{
    private readonly List<EntradaBitacora> _entradas = new();
    private readonly object _candado = new();
    private readonly string? _carpeta;
    private readonly IReloj _reloj;

    public BitacoraTecnica(IReloj reloj, string? carpeta = null)
    {
        _reloj = reloj;
        _carpeta = carpeta;
        if (_carpeta is not null) Directory.CreateDirectory(_carpeta);
    }

    public void Registrar(string categoria, string mensaje, string? correlacion = null)
    {
        var entrada = new EntradaBitacora(_reloj.Ahora, categoria, mensaje, correlacion);
        lock (_candado)
        {
            _entradas.Add(entrada);
            if (_carpeta is null) return;
            var linea = $"{entrada.Fecha:yyyy-MM-dd HH:mm:ss.fff} [{categoria}] {(correlacion is null ? "" : $"<{correlacion}> ")}{mensaje}";
            File.AppendAllText(Path.Combine(_carpeta, $"{categoria.ToLowerInvariant()}.log"), linea + Environment.NewLine);
        }
    }

    public IReadOnlyList<EntradaBitacora> Entradas(string? categoria = null)
    {
        lock (_candado)
            return _entradas.Where(e => categoria is null || e.Categoria == categoria).ToList();
    }
}
