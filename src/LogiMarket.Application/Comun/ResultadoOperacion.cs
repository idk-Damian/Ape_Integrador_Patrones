namespace LogiMarket.Application.Comun;

public sealed record ResultadoOperacion(bool Exito, string Mensaje)
{
    public static ResultadoOperacion Ok(string mensaje) => new(true, mensaje);
    public static ResultadoOperacion Falla(string mensaje) => new(false, mensaje);
}
