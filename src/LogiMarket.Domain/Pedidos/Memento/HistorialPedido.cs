using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Memento;

/// <summary>
/// Patrón MEMENTO – Historial de ediciones con Undo/Redo.
/// Máximo 5 estados. Se bloquea al confirmar pago.
/// </summary>
public sealed class HistorialPedido
{
    private readonly List<PedidoMemento> _estados = new();
    private int _posicionActual = -1;
    private bool _bloqueado;
    private const int MaxEstados = 5;

    public bool EstaBloqueado => _bloqueado;
    public bool PuedeDeshacer => !_bloqueado && _posicionActual > 0;
    public bool PuedeRehacer => !_bloqueado && _posicionActual < _estados.Count - 1;

    public void Guardar(PedidoMemento memento)
    {
        if (_bloqueado) throw new DomainException("El historial está bloqueado tras confirmar pago.");

        // Eliminar estados de redo
        if (_posicionActual < _estados.Count - 1)
            _estados.RemoveRange(_posicionActual + 1, _estados.Count - _posicionActual - 1);

        _estados.Add(memento);

        // Mantener máximo 5
        if (_estados.Count > MaxEstados)
        {
            _estados.RemoveAt(0);
        }
        else
        {
            _posicionActual++;
        }
    }

    public PedidoMemento? Deshacer()
    {
        if (!PuedeDeshacer) return null;
        _posicionActual--;
        return _estados[_posicionActual];
    }

    public PedidoMemento? Rehacer()
    {
        if (!PuedeRehacer) return null;
        _posicionActual++;
        return _estados[_posicionActual];
    }

    public void Bloquear()
    {
        _bloqueado = true;
    }

    public void Limpiar()
    {
        _estados.Clear();
        _posicionActual = -1;
        _bloqueado = false;
    }
}
