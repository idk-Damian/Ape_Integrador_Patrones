using LogiMarket.Application.Comun;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Mantenimiento;

public interface IMantenimientoBaseDatos
{
    Task InicializarAsync();
    Task RestablecerAsync();
    string Ubicacion { get; }
}

// restablecer borra todo y vuelve a la semilla; solo ADMIN y siempre con confirmacion en la UI
public sealed class ServicioMantenimiento
{
    private readonly IMantenimientoBaseDatos _base;
    private readonly IContextoUsuario _usuario;
    private readonly IBitacora _bitacora;

    public ServicioMantenimiento(IMantenimientoBaseDatos baseDatos, IContextoUsuario usuario, IBitacora bitacora)
    {
        _base = baseDatos;
        _usuario = usuario;
        _bitacora = bitacora;
    }

    public string Ubicacion => _base.Ubicacion;

    public async Task<ResultadoOperacion> RestablecerAsync()
    {
        try
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.AdministrarDemostracion);
        }
        catch (Excepciones.AccesoDenegadoException ex)
        {
            return ResultadoOperacion.Falla(ex.Message);
        }
        await _base.RestablecerAsync();
        _bitacora.Registrar(CategoriasBitacora.Seguridad, $"Base de pruebas restablecida por {_usuario.NombreUsuario}.");
        return ResultadoOperacion.Ok("La base volvió al estado inicial de la semilla.");
    }
}
