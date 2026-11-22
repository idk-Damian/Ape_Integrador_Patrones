namespace LogiMarket.Application.Comun;

// bitacora tecnica fuera de la transaccion: lo que se escribe aqui no se pierde con un rollback
public interface IBitacora
{
    void Registrar(string categoria, string mensaje, string? correlacion = null);
    IReadOnlyList<EntradaBitacora> Entradas(string? categoria = null);
}

public sealed record EntradaBitacora(DateTime Fecha, string Categoria, string Mensaje, string? Correlacion);

public static class CategoriasBitacora
{
    public const string Pago = "PAGO";
    public const string Seguridad = "SEGURIDAD";
    public const string Comando = "COMANDO";
    public const string Notificacion = "NOTIFICACION";
    public const string Outbox = "OUTBOX";
    public const string Negocio = "NEGOCIO";
}
