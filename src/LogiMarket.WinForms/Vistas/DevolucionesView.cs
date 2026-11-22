using LogiMarket.Application.Comandos;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Devoluciones;
using LogiMarket.Application.Presentacion;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 09: devolucion total con politica por motivo (Template Method), reintegro y reembolso unico
internal sealed class DevolucionesView : VistaBase
{
    private readonly DataGridView _pedidos = Ui.Grilla();
    private readonly ComboBox _motivo = Ui.Combo(170);
    private readonly CheckBox _sinUso = new() { Text = "Producto sin uso", Checked = true, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
    private readonly Label _calculo = new() { Dock = DockStyle.Top, Height = 130, Font = Ui.FuenteMonoespaciada, Padding = new Padding(6) };
    private readonly TextBox _resultado = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = Ui.FuenteMonoespaciada };

    public DevolucionesView(IServiceProvider servicios) : base(servicios)
    {
        _motivo.Items.AddRange(new object[] { MotivoDevolucion.Defecto, MotivoDevolucion.Arrepentimiento });
        _motivo.SelectedIndex = 0;
        _pedidos.SelectionChanged += async (_, _) => await EjecutarAsync(CalcularAsync);

        var barra = Ui.Barra(Ui.Etiqueta("Motivo"), _motivo, _sinUso,
            Ui.Boton("Procesar devolución", async (_, _) => await EjecutarAsync(ProcesarAsync), 170));
        var derecha = new Panel { Dock = DockStyle.Fill };
        derecha.Controls.Add(Ui.Grupo("Resultado", _resultado));
        derecha.Controls.Add(_calculo);
        derecha.Controls.Add(barra);

        Controls.Add(Ui.Dividir(Ui.Grupo("Pedidos entregados", _pedidos), derecha, true, 55));
        Controls.Add(new Label
        {
            Dock = DockStyle.Top, Height = 28, ForeColor = SystemColors.GrayText,
            Text = "DEFECTO: hasta 30 días desde la entrega. ARREPENTIMIENTO: hasta 7 días y sin uso. Se reembolsan los productos con descuento, sin el envío."
        });
    }

    public override string Nombre => "Devoluciones";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoEntregado, CambioNegocio.DevolucionProcesada);

    private PedidoResumen? Elegido => Ui.Seleccionado<PedidoResumen>(_pedidos);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        Ui.Cargar(_pedidos, await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().PedidosEnEstadoAsync("ENTREGADO")));
        await CalcularAsync();
    }

    private async Task CalcularAsync()
    {
        if (Elegido is not { } pedido) { _calculo.Text = "Seleccione un pedido entregado."; return; }
        var d = await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(pedido.Id));
        var dias = d.FechaEntrega is null ? 0 : (Unico<IReloj>().Ahora.Date - d.FechaEntrega.Value.Date).Days;
        _calculo.Text =
            $"Entregado el:        {d.FechaEntrega:dd/MM/yyyy} (hace {dias} días)\n" +
            $"Total pagado:        {pedido.Total,10:N2}\n" +
            $"Envío (no se devuelve): {-d.Tarifa,7:N2}\n" +
            $"Reembolso posible:   {d.ImporteReembolsable,10:N2}\n" +
            $"Estado devolución:   {(d.Reembolso is null ? "sin devolución" : $"ya devuelto ({d.Reembolso:N2})")}";
    }

    private async Task ProcesarAsync()
    {
        if (Elegido is not { } pedido) { Informar("Seleccione un pedido."); return; }
        if (!Confirmar($"¿Procesar la devolución total del pedido #{pedido.Id} por {_motivo.SelectedItem}?")) return;

        ProcesarDevolucionCommand? comando = null;
        await UsarAsync(sp =>
        {
            comando = new ProcesarDevolucionCommand(sp.GetRequiredService<ServicioDevoluciones>(),
                new SolicitudDevolucion(pedido.Id, (MotivoDevolucion)_motivo.SelectedItem!, _sinUso.Checked));
            return Unico<InvocadorComandos>().EjecutarAsync(comando);
        });
        var r = comando!.Resultado!;
        _resultado.Text = (r.Aceptada ? "DEVOLUCIÓN ACEPTADA" : "DEVOLUCIÓN RECHAZADA") + Environment.NewLine + r.Mensaje +
                          Environment.NewLine + Environment.NewLine + "Pasos ejecutados:" + Environment.NewLine +
                          string.Join(Environment.NewLine, r.Pasos);
        if (!r.Aceptada) return;
        await CargarDatosAsync(CambioNegocio.DevolucionProcesada);
        await AvisarAsync(CambioNegocio.DevolucionProcesada);
    }
}
