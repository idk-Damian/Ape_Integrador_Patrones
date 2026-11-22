namespace LogiMarket.Application.Comun;

// interruptores del panel de demostracion; los colaboradores simulados los consultan
// para producir fallos reales por el mismo camino que usa el sistema
public interface IControlSimulacion
{
    string ProveedorPago { get; set; }
    bool FallarCobros { get; set; }
    bool FallarNotificaciones { get; set; }
}

public static class ProveedoresPago
{
    public const string Legado = "LEGADO";
    public const string Alternativa = "ALTERNATIVA";
}
