namespace LogiMarket.Application.Pedidos.Validacion;

// eslabon de la cadena: revisa una sola cosa y, si pasa, delega en el siguiente
public abstract class ValidadorPedido
{
    private ValidadorPedido? _siguiente;

    public abstract string Nombre { get; }

    public ValidadorPedido Enlazar(ValidadorPedido siguiente)
    {
        _siguiente = siguiente;
        return siguiente;
    }

    public ResultadoValidacion Validar(ContextoValidacion contexto) => Validar(contexto, new List<string>());

    private ResultadoValidacion Validar(ContextoValidacion contexto, List<string> recorrido)
    {
        recorrido.Add(Nombre);
        var motivo = Revisar(contexto);
        if (motivo is not null)
            return new ResultadoValidacion(false, Nombre, motivo, recorrido);

        return _siguiente is null
            ? new ResultadoValidacion(true, null, null, recorrido)
            : _siguiente.Validar(contexto, recorrido);
    }

    // devuelve null si todo esta bien o el motivo del rechazo
    protected abstract string? Revisar(ContextoValidacion contexto);
}
