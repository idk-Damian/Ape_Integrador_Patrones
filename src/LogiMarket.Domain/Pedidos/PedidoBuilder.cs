using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos.Descuentos;

namespace LogiMarket.Domain.Pedidos;

/// <summary>
/// Patrón BUILDER – Ensambla un Pedido paso a paso.
/// Bloquea construcción sin cliente o sin líneas.
/// </summary>
public sealed class PedidoBuilder
{
    private int? _clienteId;
    private CategoriaCliente _categoriaCliente = CategoriaCliente.Normal;
    private readonly List<LineaPedido> _lineas = new();
    private TipoEntrega _tipoEntrega = TipoEntrega.RecogerEnTienda;
    private decimal _tarifaEnvio;
    private IEstrategiaDescuento? _estrategiaExplicita;

    public PedidoBuilder ConCliente(Cliente cliente)
    {
        if (cliente is null) throw new DomainException("El cliente no puede ser nulo.");
        _clienteId = cliente.Id;
        _categoriaCliente = cliente.Categoria;
        return this;
    }

    public PedidoBuilder ConCliente(int clienteId, CategoriaCliente categoria)
    {
        _clienteId = clienteId;
        _categoriaCliente = categoria;
        return this;
    }

    public PedidoBuilder AgregarProducto(ProductoSimple producto, int cantidad)
    {
        _lineas.Add(new LineaPedido(producto, cantidad));
        return this;
    }

    public PedidoBuilder AgregarKit(KitCompuesto kit, int cantidad)
    {
        _lineas.Add(new LineaPedido(kit, cantidad));
        return this;
    }

    public PedidoBuilder AgregarElemento(IElementoCatalogo elemento, int cantidad)
    {
        _lineas.Add(new LineaPedido(elemento, cantidad));
        return this;
    }

    public PedidoBuilder ConTipoEntrega(TipoEntrega tipoEntrega)
    {
        _tipoEntrega = tipoEntrega;
        return this;
    }

    public PedidoBuilder ConTarifaEnvio(decimal tarifa)
    {
        _tarifaEnvio = tarifa;
        return this;
    }

    public PedidoBuilder ConEstrategiaDescuento(IEstrategiaDescuento estrategia)
    {
        _estrategiaExplicita = estrategia;
        return this;
    }

    public Pedido Construir()
    {
        if (_clienteId is null)
            throw new DomainException("No se puede construir un pedido sin cliente.");
        if (_lineas.Count == 0)
            throw new DomainException("No se puede construir un pedido sin líneas.");

        int totalUnidades = 0;
        foreach (var l in _lineas)
            totalUnidades += l.TotalUnidadesFisicas;

        var estrategia = _estrategiaExplicita
            ?? SelectorDescuento.CrearEstandar().Seleccionar(totalUnidades, _categoriaCliente);

        var pedido = new Pedido(_clienteId.Value, _categoriaCliente, _lineas, _tipoEntrega, estrategia);

        if (_tarifaEnvio > 0)
            pedido.EstablecerTarifaEnvio(_tarifaEnvio);

        return pedido;
    }
}
