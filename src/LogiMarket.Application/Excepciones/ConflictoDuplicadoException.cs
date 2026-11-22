namespace LogiMarket.Application.Excepciones;

// se intento guardar un valor que debe ser unico (SKU, clave de idempotencia, devolucion por pedido)
public class ConflictoDuplicadoException : Exception
{
    public ConflictoDuplicadoException(string mensaje, Exception? interna = null) : base(mensaje, interna) { }
}
