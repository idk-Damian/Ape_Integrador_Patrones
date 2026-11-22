using LogiMarket.Application.Comandos;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 08: despacho con guia de la familia de entrega (Abstract Factory) y registro de entrega
internal sealed class DespachoView : VistaBase
{
    private readonly DataGridView _pedidos = Ui.Grilla();
    private readonly TextBox _seguimiento = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = Ui.FuenteMonoespaciada };

    public DespachoView(IServiceProvider servicios) : base(servicios)
    {
        var barra = Ui.Barra(
            Ui.Boton("Despachar", async (_, _) => await EjecutarAsync(() => EjecutarComandoAsync(sp =>
                new DespacharPedidoCommand(sp.GetRequiredService<ServicioLogistica>(), Elegido!.Id), CambioNegocio.PedidoDespachado))),
            Ui.Boton("Registrar entrega", async (_, _) => await EjecutarAsync(() => EjecutarComandoAsync(sp =>
                new EntregarPedidoCommand(sp.GetRequiredService<ServicioLogistica>(), Elegido!.Id), CambioNegocio.PedidoEntregado))),
            Ui.Boton("Ver seguimiento", async (_, _) => await EjecutarAsync(SeguimientoAsync)));
        _pedidos.SelectionChanged += async (_, _) => { if (_pedidos.Focused) await EjecutarAsync(SeguimientoAsync); };

        var izquierda = new Panel { Dock = DockStyle.Fill };
        izquierda.Controls.Add(Ui.Grupo("Pedidos pagados, despachados y entregados", _pedidos));
        izquierda.Controls.Add(barra);
        Controls.Add(Ui.Dividir(izquierda, Ui.Grupo("Seguimiento", _seguimiento), true, 60));
    }

    public override string Nombre => "Despacho y entrega";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoConfirmado, CambioNegocio.PedidoDespachado,
        CambioNegocio.PedidoEntregado, CambioNegocio.DevolucionProcesada);

    private PedidoResumen? Elegido => Ui.Seleccionado<PedidoResumen>(_pedidos);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
        => Ui.Cargar(_pedidos, await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>()
            .PedidosEnEstadoAsync("PAGADO", "DESPACHADO", "ENTREGADO")));

    private async Task EjecutarComandoAsync(Func<IServiceProvider, IComando> crear, CambioNegocio cambio)
    {
        if (Elegido is null) { Informar("Seleccione un pedido."); return; }
        var r = await UsarAsync(sp => Unico<InvocadorComandos>().EjecutarAsync(crear(sp)));
        _seguimiento.Text = r.Mensaje.Replace(" | ", Environment.NewLine);
        if (!r.Exito) { MostrarResultado(r); return; }
        await CargarDatosAsync(cambio);
        await AvisarAsync(cambio);
    }

    private async Task SeguimientoAsync()
    {
        if (Elegido is not { } pedido) return;
        var despacho = await UsarAsync(sp => sp.GetRequiredService<ServicioLogistica>().ConsultarSeguimientoAsync(pedido.Id));
        _seguimiento.Text = despacho is null
            ? $"Pedido #{pedido.Id} ({pedido.Estado}) aún no fue despachado."
            : $"Pedido #{pedido.Id} - {pedido.Cliente}{Environment.NewLine}" +
              $"Guía: {despacho.NumeroGuia}{Environment.NewLine}" +
              $"Familia: {despacho.TipoEntrega}{Environment.NewLine}" +
              $"Despachado: {despacho.FechaDespacho:dd/MM/yyyy HH:mm}{Environment.NewLine}" +
              $"Entrega estimada: {despacho.FechaEstimada:dd/MM/yyyy}{Environment.NewLine}" +
              $"Entregado: {(despacho.FechaEntrega is null ? "pendiente" : despacho.FechaEntrega.Value.ToString("dd/MM/yyyy HH:mm"))}";
    }
}
