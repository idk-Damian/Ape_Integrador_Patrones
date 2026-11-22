namespace LogiMarket.Application.Excepciones;

public class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string mensaje) : base(mensaje) { }
}
