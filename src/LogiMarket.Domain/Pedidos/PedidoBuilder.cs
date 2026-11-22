using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos.Descuentos;

namespace LogiMarket.Domain.Pedidos;

// arma el pedido por partes; no entrega nada hasta tener cliente, lineas y entrega
public sealed class PedidoBuilder
{
    private readonly CalculadoraDescuento _calculadora;
    private readonly List<LineaPedido> _lineas = new();
    private int? _clienteId;
    private CategoriaCliente _categoria;
    private IFabricaEntrega? _entrega;
    private DateTime? _fecha;

    public PedidoBuilder(CalculadoraDescuento calculadora)
    {
        _calculadora = calculadora ?? throw new DomainException("Falta la calculadora de descuentos.");
    }

    public PedidoBuilder ParaCliente(Cliente cliente)
    {
        if (cliente is null) throw new DomainException("El cliente no puede ser nulo.");
        return ParaCliente(cliente.Id, cliente.Categoria);
    }

    public PedidoBuilder ParaCliente(int clienteId, CategoriaCliente categoria)
    {
        if (clienteId <= 0) throw new DomainException("El cliente indicado no es válido.");
        _clienteId = clienteId;
        _categoria = categoria;
        return this;
    }

    public PedidoBuilder ConLinea(IElementoCatalogo elemento, int cantidad)
    {
        var existente = _lineas.Find(l => l.ElementoCodigo == elemento?.Codigo);
        if (existente is not null)
            existente.CambiarCantidad(existente.Cantidad + cantidad);
        else
            _lineas.Add(new LineaPedido(elemento!, cantidad));
        return this;
    }

    public PedidoBuilder ConEntrega(IFabricaEntrega entrega)
    {
        _entrega = entrega ?? throw new DomainException("La forma de entrega no puede ser nula.");
        return this;
    }

    public PedidoBuilder EnFecha(DateTime fecha)
    {
        _fecha = fecha;
        return this;
    }

    public Pedido Construir()
    {
        if (_clienteId is null) throw new DomainException("No se puede construir un pedido sin cliente.");
        if (_lineas.Count == 0) throw new DomainException("No se puede construir un pedido sin líneas.");
        if (_entrega is null) throw new DomainException("No se puede construir un pedido sin forma de entrega.");

        return new Pedido(_clienteId.Value, _categoria, _lineas, _entrega, _calculadora, _fecha ?? DateTime.UtcNow);
    }
}
