namespace LogiMarket.Application.Pagos;

// contrato unico de cobro; la aplicacion no conoce como cobra cada proveedor
public interface IPasarelaPago
{
    string Proveedor { get; }
    Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia);
}

public sealed record ResultadoCobro(bool Aprobado, string Proveedor, string? Referencia, string? Motivo, string? Correlacion = null)
{
    public static ResultadoCobro Aprobar(string proveedor, string referencia) => new(true, proveedor, referencia, null);
    public static ResultadoCobro Rechazar(string proveedor, string motivo) => new(false, proveedor, null, motivo);
}
