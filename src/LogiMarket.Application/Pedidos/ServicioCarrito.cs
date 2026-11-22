using LogiMarket.Application.Comun;
using LogiMarket.Application.Entregas;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Descuentos;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Application.Pedidos;

public sealed record LineaCarrito(string Codigo, string Descripcion, bool EsKit, int Cantidad, decimal PrecioUnitario,
    decimal Subtotal, int UnidadesFisicas);

public sealed record EstadoCarrito(int? ClienteId, string Cliente, IReadOnlyList<LineaCarrito> Lineas, TipoEntrega Entrega,
    decimal Subtotal, string Politica, decimal Descuento, decimal Tarifa, decimal Total, int UnidadesFisicas,
    bool PuedeDeshacer, bool PuedeRehacer, string? Origen);

// borrador en memoria de la sesion: el dominio hace los calculos y el Memento guarda las ediciones;
// solo al guardar se crea el pedido en la base
public sealed class ServicioCarrito
{
    private readonly IServiceScopeFactory _alcances;
    private readonly CalculadoraDescuento _calculadora;
    private readonly CatalogoFabricasEntrega _entregas;
    private readonly IContextoUsuario _usuario;
    private readonly IReloj _reloj;

    private Pedido? _borrador;
    private int? _clienteId;
    private string _cliente = "";
    private CategoriaCliente _categoria;
    private TipoEntrega _entrega = TipoEntrega.Estandar;
    private string? _origen;

    public ServicioCarrito(IServiceScopeFactory alcances, CalculadoraDescuento calculadora, CatalogoFabricasEntrega entregas,
        IContextoUsuario usuario, IReloj reloj)
    {
        _alcances = alcances;
        _calculadora = calculadora;
        _entregas = entregas;
        _usuario = usuario;
        _reloj = reloj;
    }

    public EstadoCarrito Estado => new(_clienteId, _cliente,
        _borrador?.Lineas.Select(l => new LineaCarrito(l.ElementoCodigo, l.ElementoNombre, l.EsKit, l.Cantidad, l.PrecioUnitario,
            l.Subtotal, l.TotalUnidadesFisicas)).ToList() ?? new List<LineaCarrito>(),
        _borrador?.TipoEntrega ?? _entrega,
        _borrador?.Subtotal ?? 0m, _borrador?.NombreDescuento ?? "-", _borrador?.Descuento ?? 0m,
        _borrador?.TarifaEnvio ?? _entregas.Obtener(_entrega).CrearTarifa().Calcular(), _borrador?.Total ?? 0m,
        _borrador?.TotalUnidadesFisicas ?? 0, _borrador?.PuedeDeshacer ?? false, _borrador?.PuedeRehacer ?? false, _origen);

    public bool TieneLineas => _borrador is not null;

    public async Task SeleccionarClienteAsync(int clienteId)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.GestionarPedidos);
        if (_borrador is not null) throw new DomainException("Vacíe el carrito antes de cambiar de cliente.");

        using var alcance = _alcances.CreateScope();
        var cliente = await alcance.ServiceProvider.GetRequiredService<IClienteRepository>().ObtenerPorIdAsync(clienteId)
            ?? throw new RecursoNoEncontradoException($"No existe el cliente {clienteId}.");
        _clienteId = cliente.Id;
        _cliente = cliente.Nombre;
        _categoria = cliente.Categoria;
    }

    public async Task AgregarAsync(string codigo, int cantidad)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.GestionarPedidos);
        if (_clienteId is null) throw new DomainException("Seleccione primero el cliente.");

        using var alcance = _alcances.CreateScope();
        var elemento = await alcance.ServiceProvider.GetRequiredService<ResolutorCatalogo>().ObtenerAsync(codigo);

        if (_borrador is null)
            _borrador = new PedidoBuilder(_calculadora).ParaCliente(_clienteId.Value, _categoria)
                .ConLinea(elemento, cantidad).ConEntrega(_entregas.Obtener(_entrega)).EnFecha(_reloj.Ahora).Construir();
        else
            _borrador.AgregarLinea(elemento, cantidad, _calculadora);
    }

    public void ModificarCantidad(string codigo, int cantidad) => Borrador().ModificarCantidad(codigo, cantidad, _calculadora);

    public void Quitar(string codigo) => Borrador().EliminarLinea(codigo, _calculadora);

    public void CambiarEntrega(TipoEntrega tipo)
    {
        _entrega = tipo;
        _borrador?.CambiarEntrega(_entregas.Obtener(tipo), _calculadora);
    }

    public bool Deshacer() => _borrador?.Deshacer(_calculadora) ?? false;
    public bool Rehacer() => _borrador?.Rehacer(_calculadora) ?? false;

    // prototype: la copia queda como nuevo carrito y el original no se toca
    public void ClonarActual()
    {
        _borrador = Borrador().ClonarComoBorrador(_reloj.Ahora);
        _origen = "copia del carrito anterior";
    }

    public async Task UsarComoPlantillaAsync(int pedidoId)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.GestionarPedidos);
        using var alcance = _alcances.CreateScope();
        var sp = alcance.ServiceProvider;
        var pedido = await sp.GetRequiredService<IPedidoRepository>().ObtenerPorIdAsync(pedidoId)
            ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
        var cliente = await sp.GetRequiredService<IClienteRepository>().ObtenerPorIdAsync(pedido.ClienteId);

        _borrador = pedido.ClonarComoBorrador(_reloj.Ahora);
        _clienteId = pedido.ClienteId;
        _cliente = cliente?.Nombre ?? "";
        _categoria = pedido.CategoriaCliente;
        _entrega = pedido.TipoEntrega;
        _origen = $"plantilla del pedido #{pedidoId}";
    }

    public async Task<int> GuardarAsync()
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.GestionarPedidos);
        var pedido = Borrador();
        using var alcance = _alcances.CreateScope();
        await alcance.ServiceProvider.GetRequiredService<IPedidoRepository>().AgregarAsync(pedido);
        await alcance.ServiceProvider.GetRequiredService<IUnitOfWork>().GuardarCambiosAsync();
        var id = pedido.Id;
        Vaciar();
        return id;
    }

    public void Vaciar()
    {
        _borrador = null;
        _origen = null;
    }

    public void Reiniciar()
    {
        Vaciar();
        _clienteId = null;
        _cliente = "";
    }

    private Pedido Borrador() => _borrador ?? throw new DomainException("El carrito está vacío.");
}
