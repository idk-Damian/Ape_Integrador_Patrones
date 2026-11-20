namespace LogiMarket.Domain.Outbox;

public class Auditoria
{
    public int Id { get; private set; }
    public string Entidad { get; private set; } = null!;
    public int EntidadId { get; private set; }
    public string Accion { get; private set; } = null!;
    public int? UsuarioId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string? Detalle { get; private set; }

    private Auditoria() { }

    public Auditoria(string entidad, int entidadId, string accion, int? usuarioId, string? detalle = null)
    {
        Entidad = entidad;
        EntidadId = entidadId;
        Accion = accion;
        UsuarioId = usuarioId;
        Detalle = detalle;
        Fecha = DateTime.UtcNow;
    }
}
