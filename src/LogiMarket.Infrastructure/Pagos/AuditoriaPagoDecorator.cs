using LogiMarket.Application.Comun;
using LogiMarket.Application.Pagos;

namespace LogiMarket.Infrastructure.Pagos;

// decorator: rodea el cobro con INICIO y FIN/ERROR bajo un mismo id de correlacion, sin cambiar el resultado
public sealed class AuditoriaPagoDecorator : IPasarelaPago
{
    private readonly IPasarelaPago _interna;
    private readonly IBitacora _bitacora;

    public AuditoriaPagoDecorator(IPasarelaPago interna, IBitacora bitacora)
    {
        _interna = interna;
        _bitacora = bitacora;
    }

    public string Proveedor => _interna.Proveedor;

    public async Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia)
    {
        var correlacion = Guid.NewGuid().ToString("N")[..12];
        _bitacora.Registrar(CategoriasBitacora.Pago, $"INICIO cobro {monto:N2} USD clave {claveIdempotencia} via {Proveedor}", correlacion);
        try
        {
            var resultado = await _interna.CobrarAsync(monto, claveIdempotencia);
            _bitacora.Registrar(CategoriasBitacora.Pago,
                resultado.Aprobado
                    ? $"FIN APROBADO ref {resultado.Referencia}"
                    : $"FIN RECHAZADO {resultado.Motivo}", correlacion);
            return resultado with { Correlacion = correlacion };
        }
        catch (Exception ex)
        {
            _bitacora.Registrar(CategoriasBitacora.Pago, $"ERROR {ex.GetType().Name}: {ex.Message}", correlacion);
            throw;
        }
    }
}
