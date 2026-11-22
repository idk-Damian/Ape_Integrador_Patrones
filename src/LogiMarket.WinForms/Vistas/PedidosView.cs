using LogiMarket.Application.Comandos;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 06: listado con filtro (Interpreter), detalle, historial de estados y acciones
internal sealed class PedidosView : VistaBase
{
    private readonly TextBox _filtro = Ui.Texto(420, "ESTADO = PAGADO AND TOTAL >= 50");
    private readonly DataGridView _pedidos = Ui.Grilla();
    private readonly DataGridView _lineas = Ui.Grilla();
    private readonly DataGridView _historial = Ui.Grilla();
    private readonly Label _detalle = new() { Dock = DockStyle.Top, Height = 46, Padding = new Padding(4) };

    public PedidosView(IServiceProvider servicios) : base(servicios)
    {
        var barraFiltro = Ui.Barra(Ui.Etiqueta("Filtro"), _filtro,
            Ui.Boton("Aplicar", async (_, _) => await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.PedidoGuardado))),
            Ui.Boton("Limpiar", async (_, _) => { _filtro.Text = ""; await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.PedidoGuardado)); }),
            new Label
            {
                AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(8, 8, 3, 3),
                Text = "Campos: ESTADO = valor, TOTAL >= número, CLIENTE = texto. Operadores AND, OR y paréntesis."
            });
        _filtro.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.PedidoGuardado)); };

        var barraAcciones = Ui.Barra(
            Ui.Boton("Validar", async (_, _) => await EjecutarAsync(ValidarAsync)),
            Ui.Boton("Cancelar pedido", async (_, _) => await EjecutarAsync(CancelarAsync)),
            Ui.Boton("Usar como plantilla en el carrito", async (_, _) => await EjecutarAsync(PlantillaAsync)));
        _pedidos.SelectionChanged += async (_, _) => await EjecutarAsync(MostrarDetalleAsync);

        var listado = new Panel { Dock = DockStyle.Fill };
        listado.Controls.Add(Ui.Grupo("Pedidos", _pedidos));
        listado.Controls.Add(barraAcciones);

        var detalle = new Panel { Dock = DockStyle.Fill };
        detalle.Controls.Add(Ui.Dividir(Ui.Grupo("Líneas (instantánea de precios y kits)", _lineas), Ui.Grupo("Historial de estados", _historial), true, 60));
        detalle.Controls.Add(_detalle);

        Controls.Add(Ui.Dividir(listado, detalle, false, 52));
        Controls.Add(barraFiltro);
    }

    public override string Nombre => "Pedidos e historial";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoGuardado, CambioNegocio.PedidoValidado,
        CambioNegocio.PedidoConfirmado, CambioNegocio.PedidoCancelado, CambioNegocio.PedidoDespachado, CambioNegocio.PedidoEntregado,
        CambioNegocio.DevolucionProcesada, CambioNegocio.ClientesModificados);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var filtro = _filtro.Text;
        Ui.Cargar(_pedidos, await UsarAsync(sp => sp.GetRequiredService<ServicioConsultasPedidos>().FiltrarAsync(filtro)));
        await MostrarDetalleAsync();
    }

    private PedidoResumen? Elegido => Ui.Seleccionado<PedidoResumen>(_pedidos);

    private async Task MostrarDetalleAsync()
    {
        if (Elegido is not { } pedido)
        {
            _detalle.Text = "";
            Ui.Cargar(_lineas, Array.Empty<LineaDetalle>());
            Ui.Cargar(_historial, Array.Empty<CambioEstadoDto>());
            return;
        }
        var d = await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(pedido.Id));
        Ui.Cargar(_lineas, d.Lineas);
        Ui.Cargar(_historial, d.Historial);
        _detalle.Text =
            $"Pedido #{pedido.Id} de {pedido.Cliente}: subtotal {d.Subtotal:N2} - descuento {d.Descuento:N2} ({d.Politica}) + envío {d.Tarifa:N2} = {pedido.Total:N2} USD\n" +
            $"Cobro: {d.ReferenciaCobro ?? "-"}   Guía: {d.Guia ?? "-"}   Entrega estimada: {d.FechaEstimada:dd/MM/yyyy}   Reembolso: {(d.Reembolso is null ? "-" : d.Reembolso.Value.ToString("N2"))}";
    }

    private async Task ValidarAsync()
    {
        if (Elegido is not { } pedido) { Informar("Seleccione un pedido."); return; }
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioPedidos>().ValidarAsync(pedido.Id));
        var recorrido = string.Join(" -> ", r.Recorrido);
        if (r.Valido)
        {
            Informar($"Pedido #{pedido.Id} validado.\n\nCadena recorrida: {recorrido}");
            await CargarDatosAsync(CambioNegocio.PedidoValidado);
            await AvisarAsync(CambioNegocio.PedidoValidado);
        }
        else
            MessageBox.Show($"La validación se detuvo en \"{r.EslabonFallido}\".\n{r.Motivo}\n\nCadena recorrida: {recorrido}",
                "Pedido no válido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private async Task CancelarAsync()
    {
        if (Elegido is not { } pedido) { Informar("Seleccione un pedido."); return; }
        if (!Confirmar($"¿Cancelar el pedido #{pedido.Id}? Esta acción no se puede deshacer.")) return;
        var r = await UsarAsync(sp => Unico<InvocadorComandos>().EjecutarAsync(new CancelarPedidoCommand(sp.GetRequiredService<ServicioPedidos>(), pedido.Id)));
        MostrarResultado(r);
        if (!r.Exito) return;
        await CargarDatosAsync(CambioNegocio.PedidoCancelado);
        await AvisarAsync(CambioNegocio.PedidoCancelado);
    }

    private async Task PlantillaAsync()
    {
        if (Elegido is not { } pedido) { Informar("Seleccione un pedido en BORRADOR."); return; }
        var carrito = Unico<ServicioCarrito>();
        if (carrito.TieneLineas && !Confirmar("El carrito tiene líneas sin guardar. ¿Reemplazarlas por la plantilla?")) return;
        await carrito.UsarComoPlantillaAsync(pedido.Id);
        await AvisarAsync(CambioNegocio.CarritoModificado);
        Informar($"La copia del pedido #{pedido.Id} quedó en Carrito y borrador. El pedido original no cambia.");
    }
}
