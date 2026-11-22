using LogiMarket.Application.Consultas;
using LogiMarket.Application.Metricas;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 02: resumen de la sesion (Singleton de metricas) y del estado de la base
internal sealed class PanelGeneralView : VistaBase
{
    private readonly FlowLayoutPanel _indicadores = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
    private readonly DataGridView _estados = Ui.Grilla();
    private readonly DataGridView _alertas = Ui.Grilla();

    public PanelGeneralView(IServiceProvider servicios) : base(servicios)
    {
        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        tabla.Controls.Add(Ui.Grupo("Pedidos por estado", _estados), 0, 0);
        tabla.Controls.Add(Ui.Grupo($"Alertas de stock (≤ {ServicioConsultas.UmbralStockBajo} unidades)", _alertas), 1, 0);

        Controls.Add(tabla);
        Controls.Add(_indicadores);
        Controls.Add(new Label { Text = "Indicadores de esta sesión", Dock = DockStyle.Top, Height = 24, ForeColor = SystemColors.GrayText });
    }

    public override string Nombre => "Panel general";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoGuardado, CambioNegocio.PedidoValidado,
        CambioNegocio.PedidoConfirmado, CambioNegocio.PedidoCancelado, CambioNegocio.PedidoDespachado, CambioNegocio.PedidoEntregado,
        CambioNegocio.DevolucionProcesada, CambioNegocio.CatalogoModificado, CambioNegocio.EventosProcesados);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var panel = await UsarAsync(sp => sp.GetRequiredService<ServicioConsultas>().PanelAsync());
        var metricas = Unico<MetricasSesion>();

        _indicadores.Controls.Clear();
        Indicador("Pagos", metricas.PedidosPagados.ToString());
        Indicador("Despachos", metricas.Despachos.ToString());
        Indicador("Devoluciones", metricas.Devoluciones.ToString());
        Indicador("Avisos enviados", metricas.Notificaciones.ToString());
        Indicador("Eventos procesados", metricas.EventosProcesados.ToString());
        Indicador("Eventos pendientes", panel.EventosPendientes.ToString());
        Indicador("Ventas cobradas USD", panel.VentasCobradas.ToString("N2"));

        Ui.Cargar(_estados, panel.PorEstado);
        Ui.Cargar(_alertas, panel.StockBajo);
    }

    private void Indicador(string titulo, string valor)
    {
        // las cajas se crean despues del autoescalado del formulario, por eso se pasan a pixeles del monitor
        var caja = new Panel
        {
            Width = LogicalToDeviceUnits(150), Height = LogicalToDeviceUnits(64), BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, LogicalToDeviceUnits(8), LogicalToDeviceUnits(8))
        };
        caja.Controls.Add(new Label { Text = valor, Font = Ui.FuenteTitulo, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
        caja.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Top, Height = LogicalToDeviceUnits(22), TextAlign = ContentAlignment.MiddleCenter, ForeColor = SystemColors.GrayText });
        _indicadores.Controls.Add(caja);
    }
}
