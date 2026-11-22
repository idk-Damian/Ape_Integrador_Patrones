using LogiMarket.Application.Comun;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Pagos;

// proxy de proteccion: si el rol no puede cobrar, la llamada nunca llega al decorador ni al proveedor
public sealed class PagoAutorizadoProxy : IPasarelaPago
{
    private readonly IPasarelaPago _real;
    private readonly IContextoUsuario _usuario;
    private readonly IBitacora _bitacora;

    public PagoAutorizadoProxy(IPasarelaPago real, IContextoUsuario usuario, IBitacora bitacora)
    {
        _real = real;
        _usuario = usuario;
        _bitacora = bitacora;
    }

    public string Proveedor => _real.Proveedor;

    public Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia)
    {
        if (!PoliticaPermisos.Permite(_usuario.Rol, Operacion.CobrarPedido))
        {
            _bitacora.Registrar(CategoriasBitacora.Seguridad,
                $"Cobro denegado a '{_usuario.NombreUsuario}' ({_usuario.Rol?.ToString() ?? "sin sesión"}) por {monto:N2}, clave {claveIdempotencia}.");
            throw new AccesoDenegadoException("Solo VENTAS o ADMIN pueden cobrar pedidos.");
        }
        return _real.CobrarAsync(monto, claveIdempotencia);
    }
}
