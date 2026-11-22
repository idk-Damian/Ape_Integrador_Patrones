namespace LogiMarket.Application.Pagos;

// reembolso simulado; la clave evita devolver dos veces el mismo dinero
public interface IPasarelaReembolso
{
    Task<string> ReembolsarAsync(string referenciaCobro, decimal monto, string claveIdempotencia);
}
