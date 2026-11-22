using LogiMarket.Application.Comun;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Comandos;

public sealed record RegistroComando(DateTime Fecha, string Usuario, string Comando, string Entrada, bool Exito, string Resultado);

// invocador: ejecuta cualquier IComando y deja registro de quien, que, con que datos y como termino
public sealed class InvocadorComandos
{
    private readonly List<RegistroComando> _historial = new();
    private readonly object _candado = new();
    private readonly IContextoUsuario _usuario;
    private readonly IReloj _reloj;
    private readonly IBitacora _bitacora;

    public InvocadorComandos(IContextoUsuario usuario, IReloj reloj, IBitacora bitacora)
    {
        _usuario = usuario;
        _reloj = reloj;
        _bitacora = bitacora;
    }

    public IReadOnlyList<RegistroComando> Historial
    {
        get { lock (_candado) return _historial.ToList(); }
    }

    public async Task<ResultadoOperacion> EjecutarAsync(IComando comando)
    {
        var resultado = await comando.EjecutarAsync();
        var registro = new RegistroComando(_reloj.Ahora, _usuario.NombreUsuario, comando.Nombre, comando.Entrada,
            resultado.Exito, resultado.Mensaje);

        lock (_candado) _historial.Add(registro);
        _bitacora.Registrar(CategoriasBitacora.Comando,
            $"{registro.Comando}({registro.Entrada}) por {registro.Usuario}: {(registro.Exito ? "OK" : "FALLA")} - {registro.Resultado}");
        return resultado;
    }
}
