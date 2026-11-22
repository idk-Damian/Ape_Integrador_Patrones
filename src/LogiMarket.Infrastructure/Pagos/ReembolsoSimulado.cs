using LogiMarket.Application.Comun;
using LogiMarket.Application.Pagos;

namespace LogiMarket.Infrastructure.Pagos;

// reembolso simulado e idempotente por clave: la misma devolucion nunca devuelve dinero dos veces
public sealed class ReembolsoSimulado : IPasarelaReembolso
{
    private readonly Dictionary<string, string> _emitidos = new();
    private readonly object _candado = new();
    private readonly IBitacora _bitacora;

    public ReembolsoSimulado(IBitacora bitacora) => _bitacora = bitacora;

    public Task<string> ReembolsarAsync(string referenciaCobro, decimal monto, string claveIdempotencia)
    {
        if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El reembolso debe ser positivo.");
        lock (_candado)
        {
            if (!_emitidos.TryGetValue(claveIdempotencia, out var referencia))
            {
                referencia = "RMB-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
                _emitidos[claveIdempotencia] = referencia;
                _bitacora.Registrar(CategoriasBitacora.Pago, $"REEMBOLSO {monto:N2} USD sobre {referenciaCobro} -> {referencia}");
            }
            return Task.FromResult(referencia);
        }
    }
}
