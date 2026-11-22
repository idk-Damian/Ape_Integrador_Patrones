namespace LogiMarket.Application.Excepciones;

// otro proceso modifico los mismos datos antes de guardar (por ejemplo, el stock)
public class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(string mensaje, Exception? interna = null) : base(mensaje, interna) { }
}
