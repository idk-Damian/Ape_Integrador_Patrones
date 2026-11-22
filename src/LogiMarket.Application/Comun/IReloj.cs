namespace LogiMarket.Application.Comun;

// la hora se inyecta para poder probar plazos de devolucion sin esperar dias
public interface IReloj
{
    DateTime Ahora { get; }
}
