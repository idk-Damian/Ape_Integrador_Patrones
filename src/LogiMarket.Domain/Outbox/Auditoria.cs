namespace LogiMarket.Domain.Outbox;

// registro de solo escritura para negocio, seguridad y pagos
public class Auditoria
{
    public int Id { get; private set; }
    public string Categoria { get; private set; } = null!;
    public string Accion { get; private set; } = null!;
    public string Referencia { get; private set; } = null!;
    public string Usuario { get; private set; } = null!;
    public string? Detalle { get; private set; }
    public string? Correlacion { get; private set; }
    public DateTime Fecha { get; private set; }

    private Auditoria() { }

    public Auditoria(string categoria, string accion, string referencia, string usuario, DateTime fecha,
        string? detalle = null, string? correlacion = null)
    {
        Categoria = categoria;
        Accion = accion;
        Referencia = referencia;
        Usuario = usuario;
        Fecha = fecha;
        Detalle = detalle;
        Correlacion = correlacion;
    }
}
