using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Memento;

// cuidador: guarda hasta cinco fotos del borrador con deshacer y rehacer
public sealed class HistorialPedido
{
    public const int MaxEstados = 5;

    private readonly List<PedidoMemento> _estados = new();
    private int _posicion = -1;

    public bool EstaBloqueado { get; private set; }
    public int Cantidad => _estados.Count;
    public bool PuedeDeshacer => !EstaBloqueado && _posicion > 0;
    public bool PuedeRehacer => !EstaBloqueado && _posicion < _estados.Count - 1;

    public void Guardar(PedidoMemento memento)
    {
        if (EstaBloqueado) throw new DomainException("El historial está bloqueado: el pedido ya fue confirmado.");

        if (_posicion < _estados.Count - 1)
            _estados.RemoveRange(_posicion + 1, _estados.Count - _posicion - 1);

        _estados.Add(memento);
        if (_estados.Count > MaxEstados)
            _estados.RemoveAt(0);

        _posicion = _estados.Count - 1;
    }

    public PedidoMemento? Deshacer()
    {
        if (!PuedeDeshacer) return null;
        return _estados[--_posicion];
    }

    public PedidoMemento? Rehacer()
    {
        if (!PuedeRehacer) return null;
        return _estados[++_posicion];
    }

    public void LimpiarYBloquear()
    {
        _estados.Clear();
        _posicion = -1;
        EstaBloqueado = true;
    }
}
