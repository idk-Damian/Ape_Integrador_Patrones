using LogiMarket.Application.Comun;
using LogiMarket.Application.Pagos;
using LogiMarket.Infrastructure.Pagos.Legado;

namespace LogiMarket.Infrastructure.Pagos;

// adapter: traduce CobrarAsync(decimal, clave) a CobrarCentavos(long, string) y la respuesta de texto a ResultadoCobro
public sealed class PasarelaLegadaAdapter : IPasarelaPago
{
    private readonly PasarelaLegadaSimulada _legado;
    private readonly IControlSimulacion _control;
    private readonly IBitacora _bitacora;

    public PasarelaLegadaAdapter(PasarelaLegadaSimulada legado, IControlSimulacion control, IBitacora bitacora)
    {
        _legado = legado;
        _control = control;
        _bitacora = bitacora;
    }

    public string Proveedor => ProveedoresPago.Legado;

    public Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia)
    {
        ContratoCobro.ValidarEntrada(monto, claveIdempotencia);

        long centavos = (long)decimal.Round(monto * 100m, 0, MidpointRounding.AwayFromZero);
        _legado.RechazarTodo = _control.FallarCobros;
        var respuesta = _legado.CobrarCentavos(centavos, claveIdempotencia);
        _bitacora.Registrar(CategoriasBitacora.Pago, $"PASARELA {Proveedor} CobrarCentavos({centavos}, {claveIdempotencia}) -> {respuesta}");

        var partes = respuesta.Split(';', 2);
        var resultado = partes[0] == "OK"
            ? ResultadoCobro.Aprobar(Proveedor, partes[1])
            : ResultadoCobro.Rechazar(Proveedor, partes.Length > 1 ? partes[1] : "SIN_DETALLE");
        return Task.FromResult(resultado);
    }
}
