using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Clientes;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Presentacion;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 05: borrador en memoria con Builder, Memento (deshacer/rehacer) y Prototype (clonar)
internal sealed class CarritoView : VistaBase
{
    private readonly ServicioCarrito _carrito;
    private readonly ComboBox _clientes = Ui.Combo(240);
    private readonly ComboBox _entrega = Ui.Combo(120);
    private readonly ComboBox _elementos = Ui.Combo(320);
    private readonly NumericUpDown _cantidad = Ui.Numero(1, 500, 0, 70);
    private readonly NumericUpDown _nuevaCantidad = Ui.Numero(1, 500, 0, 70);
    private readonly DataGridView _lineas = Ui.Grilla();
    private readonly Label _totales = new() { Dock = DockStyle.Top, Height = 150, Font = Ui.FuenteMonoespaciada, Padding = new Padding(6) };
    private readonly Label _origen = new() { Dock = DockStyle.Top, Height = 40, ForeColor = SystemColors.GrayText, Padding = new Padding(6) };
    private readonly Button _deshacer;
    private readonly Button _rehacer;

    public CarritoView(IServiceProvider servicios) : base(servicios)
    {
        _carrito = Unico<ServicioCarrito>();
        _entrega.Items.AddRange(new object[] { new Opcion<TipoEntrega>(TipoEntrega.Estandar, "Estándar (3 días)"), new Opcion<TipoEntrega>(TipoEntrega.Expres, "Exprés (1 día)") });
        _entrega.SelectedIndex = 0;
        _entrega.SelectedIndexChanged += async (_, _) => await Accion(() =>
        {
            if (_entrega.SelectedItem is Opcion<TipoEntrega> o && o.Valor != _carrito.Estado.Entrega) _carrito.CambiarEntrega(o.Valor);
        });

        var barraCliente = Ui.Barra(Ui.Etiqueta("Cliente"), _clientes,
            Ui.Boton("Seleccionar cliente", async (_, _) => await EjecutarAsync(SeleccionarClienteAsync)),
            Ui.Etiqueta("Entrega"), _entrega);
        var barraLinea = Ui.Barra(Ui.Etiqueta("Producto o kit"), _elementos, Ui.Etiqueta("Cantidad"), _cantidad,
            Ui.Boton("Agregar", async (_, _) => await EjecutarAsync(AgregarAsync)));

        _deshacer = Ui.Boton("Deshacer", async (_, _) => await Accion(() => _carrito.Deshacer()), 150);
        _rehacer = Ui.Boton("Rehacer", async (_, _) => await Accion(() => _carrito.Rehacer()), 150);
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(4) };
        Ui.Agregar(acciones,
            Ui.Etiqueta("Nueva cantidad de la línea"), _nuevaCantidad,
            Ui.Boton("Cambiar cantidad", async (_, _) => await Accion(() => _carrito.ModificarCantidad(LineaElegida().Codigo, (int)_nuevaCantidad.Value)), 150),
            Ui.Boton("Quitar línea", async (_, _) => await Accion(() => _carrito.Quitar(LineaElegida().Codigo)), 150),
            _deshacer, _rehacer,
            Ui.Boton("Clonar carrito", async (_, _) => await Accion(_carrito.ClonarActual), 150),
            Ui.Boton("Vaciar", async (_, _) => await Accion(() => { if (Confirmar("¿Vaciar el carrito? Se perderán las líneas no guardadas.")) _carrito.Vaciar(); }), 150),
            Ui.Boton("Guardar borrador", async (_, _) => await EjecutarAsync(GuardarAsync), 150));

        var derecha = new Panel { Dock = DockStyle.Fill };
        derecha.Controls.Add(acciones);
        derecha.Controls.Add(_origen);
        derecha.Controls.Add(_totales);

        Controls.Add(Ui.Dividir(Ui.Grupo("Líneas del borrador", _lineas), Ui.Grupo("Totales y acciones", derecha), true, 62));
        Controls.Add(barraLinea);
        Controls.Add(barraCliente);
    }

    public override string Nombre => "Carrito y borrador";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.CatalogoModificado, CambioNegocio.ClientesModificados,
        CambioNegocio.CarritoModificado);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        if (cambio == CambioNegocio.DatosRestablecidos) _carrito.Reiniciar();

        var (clientes, productos, kits) = await UsarAsync(async sp =>
        {
            var catalogo = sp.GetRequiredService<ServicioCatalogo>();
            return (await sp.GetRequiredService<ServicioClientes>().ListarAsync(), await catalogo.ListarProductosAsync(), await catalogo.ListarKitsAsync());
        });

        _clientes.Items.Clear();
        foreach (var c in clientes.Where(c => c.Activo))
            _clientes.Items.Add(new Opcion<int>(c.Id, $"{c.Nombre} ({c.Categoria})"));
        _elementos.Items.Clear();
        foreach (var p in productos) _elementos.Items.Add(new Opcion<string>(p.Sku, $"{p.Sku} - {p.Nombre} - {p.Precio:N2} USD (stock {p.Stock})"));
        foreach (var k in kits) _elementos.Items.Add(new Opcion<string>(k.Codigo, $"{k.Codigo} - {k.Nombre} - {k.Precio:N2} USD"));
        if (_elementos.Items.Count > 0) _elementos.SelectedIndex = 0;
        Pintar();
    }

    private async Task SeleccionarClienteAsync()
    {
        if (_clientes.SelectedItem is not Opcion<int> cliente) { Informar("Elija un cliente de la lista."); return; }
        await _carrito.SeleccionarClienteAsync(cliente.Valor);
        Pintar();
    }

    private async Task AgregarAsync()
    {
        if (_elementos.SelectedItem is not Opcion<string> elemento) return;
        await _carrito.AgregarAsync(elemento.Valor, (int)_cantidad.Value);
        Pintar();
    }

    private async Task GuardarAsync()
    {
        var id = await _carrito.GuardarAsync();
        Pintar();
        Informar($"Borrador guardado como pedido #{id}. Puede validarlo en Pedidos o cobrarlo en Cobro.");
        await AvisarAsync(CambioNegocio.PedidoGuardado);
    }

    private Task Accion(Action accion) => EjecutarAsync(() =>
    {
        accion();
        Pintar();
        return Task.CompletedTask;
    });

    private LineaCarrito LineaElegida()
        => Ui.Seleccionado<LineaCarrito>(_lineas) ?? throw new Domain.Exceptions.DomainException("Seleccione una línea del borrador.");

    private void Pintar()
    {
        var e = _carrito.Estado;
        Ui.Cargar(_lineas, e.Lineas);
        _totales.Text =
            $"Cliente:     {(e.ClienteId is null ? "(sin seleccionar)" : e.Cliente)}\n" +
            $"Subtotal:    {e.Subtotal,10:N2}\n" +
            $"Descuento:   {-e.Descuento,10:N2}  {e.Politica}\n" +
            $"Envío:       {e.Tarifa,10:N2}  {e.Entrega}\n" +
            $"TOTAL USD:   {e.Total,10:N2}\n" +
            $"Unidades físicas: {e.UnidadesFisicas}";
        _origen.Text = e.Origen is null ? "" : $"Borrador creado como {e.Origen}.";
        _deshacer.Enabled = e.PuedeDeshacer;
        _rehacer.Enabled = e.PuedeRehacer;
        var entrega = _entrega.Items.Cast<Opcion<TipoEntrega>>().FirstOrDefault(o => o.Valor == e.Entrega);
        if (entrega is not null && !Equals(_entrega.SelectedItem, entrega)) _entrega.SelectedItem = entrega;
    }
}
