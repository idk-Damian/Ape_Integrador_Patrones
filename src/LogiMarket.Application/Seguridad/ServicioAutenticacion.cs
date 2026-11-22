using LogiMarket.Application.Comun;
using LogiMarket.Application.Interfaces;

namespace LogiMarket.Application.Seguridad;

public sealed class ServicioAutenticacion
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly SesionUsuario _sesion;
    private readonly IBitacora _bitacora;

    public ServicioAutenticacion(IUsuarioRepository usuarios, IPasswordHasher hasher, SesionUsuario sesion, IBitacora bitacora)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _sesion = sesion;
        _bitacora = bitacora;
    }

    public async Task<ResultadoOperacion> IniciarSesionAsync(string nombreUsuario, string password)
    {
        var usuario = await _usuarios.ObtenerPorNombreAsync(nombreUsuario);
        // mismo mensaje para usuario inexistente o clave incorrecta
        if (usuario is null || !usuario.Activo || !_hasher.Verificar(password, usuario.PasswordHash, usuario.Salt))
        {
            _bitacora.Registrar(CategoriasBitacora.Seguridad, $"Intento de acceso fallido para '{nombreUsuario}'.");
            return ResultadoOperacion.Falla("Usuario o contraseña incorrectos.");
        }

        _sesion.Iniciar(usuario.NombreUsuario, usuario.Rol);
        _bitacora.Registrar(CategoriasBitacora.Seguridad, $"Inicio de sesión de '{usuario.NombreUsuario}' ({usuario.Rol}).");
        return ResultadoOperacion.Ok($"Bienvenido, {usuario.NombreUsuario}.");
    }

    public void CerrarSesion()
    {
        _bitacora.Registrar(CategoriasBitacora.Seguridad, $"Cierre de sesión de '{_sesion.NombreUsuario}'.");
        _sesion.Cerrar();
    }
}
