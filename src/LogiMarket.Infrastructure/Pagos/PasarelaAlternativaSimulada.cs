using LogiMarket.Application.Comun;
using LogiMarket.Application.Pagos;

namespace LogiMarket.Infrastructure.Pagos;

// segundo proveedor que ya habla el contrato nuevo; se cambia por DI sin tocar los casos de uso
public sealed class PasarelaAlternativaSimulada : IPasarelaPago
{
    private readonly Dictionary<string, ResultadoCobro> _aprobados = new();
    private readonly object _candado = new();
    private readonly IControlSimulacion _control;
    private readonly IBitacora _bitacora;

    public PasarelaAlternativaSimulada(IControlSimulacion control, IBitacora bitacora)
    {
        _control = control;
        _bitacora = bitacora;
    }

    public string Proveedor => ProveedoresPago.Alternativa;

    public Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia)
    {
        ContratoCobro.ValidarEntrada(monto, claveIdempotencia);

        ResultadoCobro resultado;
        lock (_candado)
        {
            if (_aprobados.TryGetValue(claveIdempotencia, out var previo))
                resultado = previo;
            else if (_control.FallarCobros)
                resultado = ResultadoCobro.Rechazar(Proveedor, "FONDOS_INSUFICIENTES");
            else
            {
                resultado = ResultadoCobro.Aprobar(Proveedor, "ALT-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant());
                _aprobados[claveIdempotencia] = resultado;
            }
        }

        _bitacora.Registrar(CategoriasBitacora.Pago,
            $"PASARELA {Proveedor} autorizar({monto:N2}, {claveIdempotencia}) -> {(resultado.Aprobado ? resultado.Referencia : resultado.Motivo)}");
        return Task.FromResult(resultado);
    }
}
