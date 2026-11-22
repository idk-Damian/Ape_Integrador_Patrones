using LogiMarket.Application.Comandos;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Notificaciones;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 07: resumen de importes, proveedor, canal de aviso y cobro protegido (Command -> Facade -> Proxy)
internal sealed class CobroView : VistaBase
{
    private readonly ComboBox _pedidos = Ui.Combo(420);
    private readonly TextBox _clave = Ui.Texto(260);
    private readonly ComboBox _proveedor = Ui.Combo(160);
    private readonly ComboBox _canal = Ui.Combo(120);
    private readonly Label _resumen = new() { Dock = DockStyle.Top, Height = 120, Font = Ui.FuenteMonoespaciada, Padding = new Padding(6) };
    private readonly TextBox _salida = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = Ui.FuenteMonoespaciada };

    public CobroView(IServiceProvider servicios) : base(servicios)
    {
        var control = Unico<IControlSimulacion>();
        var configuracion = Unico<ConfiguracionNotificaciones>();
        _proveedor.Items.AddRange(new object[] { ProveedoresPago.Legado, ProveedoresPago.Alternativa });
        _proveedor.SelectedItem = control.ProveedorPago;
        _proveedor.SelectedIndexChanged += (_, _) => control.ProveedorPago = (string)_proveedor.SelectedItem!;
        _canal.Items.AddRange(new object[] { CanalesNotificacion.Email, CanalesNotificacion.Sms });
        _canal.SelectedItem = configuracion.Canal;
        _canal.SelectedIndexChanged += (_, _) => configuracion.Canal = (string)_canal.SelectedItem!;
        _pedidos.SelectedIndexChanged += async (_, _) => await EjecutarAsync(MostrarResumenAsync);

        var formulario = Ui.Formulario(
            ("Pedido por cobrar", _pedidos),
            ("Clave de idempotencia", _clave),
            ("Proveedor de pago", _proveedor),
            ("Canal de aviso", _canal));
        var botones = Ui.Barra(
            Ui.Boton("Nueva clave", (_, _) => NuevaClave()),
            Ui.Boton("Confirmar y cobrar", async (_, _) => await EjecutarAsync(ConfirmarAsync), 170));

        var superior = new Panel { Dock = DockStyle.Top, Height = 300 };
        superior.Controls.Add(_resumen);
        superior.Controls.Add(botones);
        superior.Controls.Add(formulario);

        Controls.Add(Ui.Grupo("Resultado y traza del cobro", _salida));
        Controls.Add(superior);
    }

    public override string Nombre => "Confirmación y cobro";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoGuardado, CambioNegocio.PedidoValidado,
        CambioNegocio.PedidoCancelado, CambioNegocio.PedidoConfirmado);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var anterior = (_pedidos.SelectedItem as Opcion<int>)?.Valor;
        var pendientes = await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().PedidosEnEstadoAsync("BORRADOR", "VALIDADO"));
        _pedidos.Items.Clear();
        foreach (var p in pendientes)
            _pedidos.Items.Add(new Opcion<int>(p.Id, $"#{p.Id} - {p.Cliente} - {p.Estado} - {p.Total:N2} USD"));
        var elegido = _pedidos.Items.Cast<Opcion<int>>().FirstOrDefault(o => o.Valor == anterior);
        if (elegido is not null) _pedidos.SelectedItem = elegido;
        else if (_pedidos.Items.Count > 0) _pedidos.SelectedIndex = 0;
        else _resumen.Text = "No hay pedidos en BORRADOR o VALIDADO.";
    }

    private void NuevaClave()
    {
        if (_pedidos.SelectedItem is Opcion<int> p)
            _clave.Text = $"COB-{p.Valor}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    private async Task MostrarResumenAsync()
    {
        if (_pedidos.SelectedItem is not Opcion<int> pedido) return;
        var d = await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(pedido.Valor));
        _resumen.Text =
            $"Cliente:     {d.Resumen.Cliente}\n" +
            $"Subtotal:    {d.Subtotal,10:N2}\n" +
            $"Descuento:   {-d.Descuento,10:N2}  {d.Politica}\n" +
            $"Envío:       {d.Tarifa,10:N2}  {d.Resumen.Entrega}\n" +
            $"TOTAL USD:   {d.Resumen.Total,10:N2}";
        NuevaClave();
    }

    private async Task ConfirmarAsync()
    {
        if (_pedidos.SelectedItem is not Opcion<int> pedido) { Informar("Seleccione un pedido."); return; }
        if (!Confirmar($"Se cobrará el pedido #{pedido.Valor} con la clave {_clave.Text} usando {_proveedor.SelectedItem}.\n¿Continuar?",
                "Confirmar cobro")) return;

        var bitacora = Unico<IBitacora>();
        int desde = bitacora.Entradas(CategoriasBitacora.Pago).Count + bitacora.Entradas(CategoriasBitacora.Seguridad).Count;
        var clave = _clave.Text;

        ConfirmarPedidoCommand? comando = null;
        await UsarAsync(sp =>
        {
            comando = new ConfirmarPedidoCommand(sp.GetRequiredService<ConfirmacionPedidoFacade>(), pedido.Valor, clave);
            return Unico<InvocadorComandos>().EjecutarAsync(comando);
        });
        var r = comando!.Resultado!;

        var traza = bitacora.Entradas().Where(e => e.Categoria is CategoriasBitacora.Pago or CategoriasBitacora.Seguridad)
            .Skip(desde).Select(e => $"{e.Fecha:HH:mm:ss} [{e.Categoria}] {(e.Correlacion is null ? "" : $"<{e.Correlacion}> ")}{e.Mensaje}");
        _salida.Text = (r.Exito ? "COBRO APROBADO" : "COBRO NO REALIZADO") + Environment.NewLine + r.Mensaje + Environment.NewLine +
                       (r.Validacion is null ? "" : $"Cadena de validación: {string.Join(" -> ", r.Validacion.Recorrido)}{Environment.NewLine}") +
                       Environment.NewLine + string.Join(Environment.NewLine, traza);

        if (r.Exito && !r.EsRepeticion)
        {
            await AvisarAsync(CambioNegocio.PedidoConfirmado);
            await CargarDatosAsync(CambioNegocio.PedidoConfirmado);
        }
    }
}
