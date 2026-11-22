using LogiMarket.Application.Comun;

namespace LogiMarket.Infrastructure.Comun;

// estado compartido del panel de demostracion
public sealed class ControlSimulacion : IControlSimulacion
{
    public string ProveedorPago { get; set; } = ProveedoresPago.Legado;
    public bool FallarCobros { get; set; }
    public bool FallarNotificaciones { get; set; }
}
