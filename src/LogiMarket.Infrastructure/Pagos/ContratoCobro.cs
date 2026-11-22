namespace LogiMarket.Infrastructure.Pagos;

// precondiciones comunes: ambos proveedores fallan igual ante la misma entrada (LSP)
internal static class ContratoCobro
{
    public static void ValidarEntrada(decimal monto, string claveIdempotencia)
    {
        if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto a cobrar debe ser positivo.");
        if (string.IsNullOrWhiteSpace(claveIdempotencia))
            throw new ArgumentException("La clave de idempotencia es obligatoria.", nameof(claveIdempotencia));
    }
}
