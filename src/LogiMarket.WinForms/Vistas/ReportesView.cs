using System.Diagnostics;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 11: comprobantes (Bridge: tipo de documento x medio de salida) y resumen de pedidos
internal sealed class ReportesView : VistaBase
{
    private readonly ComboBox _pedido = Ui.Combo(380);
    private readonly ComboBox _tipo = Ui.Combo(150);
    private readonly ComboBox _medio = Ui.Combo(150);
    private readonly Label _ruta = new() { AutoSize = true, Margin = new Padding(8, 8, 3, 3) };
    private readonly TextBox _vista = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = Ui.FuenteMonoespaciada };
    private readonly DataGridView _resumen = Ui.Grilla();
    private string? _ultimaRuta;

    public ReportesView(IServiceProvider servicios) : base(servicios)
    {
        _tipo.Items.AddRange(new object[] { "Venta", "Devolución" });
        _tipo.SelectedIndex = 0;
        _medio.Items.AddRange(new object[] { new Opcion<string>(MediosSalida.VistaPrevia, "Vista previa"), new Opcion<string>(MediosSalida.ArchivoTxt, "Archivo TXT") });
        _medio.SelectedIndex = 0;

        var barra = Ui.Barra(Ui.Etiqueta("Pedido"), _pedido, Ui.Etiqueta("Comprobante"), _tipo, Ui.Etiqueta("Salida"), _medio,
            Ui.Boton("Emitir", async (_, _) => await EjecutarAsync(EmitirAsync)),
            Ui.Boton("Abrir carpeta", (_, _) => AbrirCarpeta()), _ruta);

        Controls.Add(Ui.Dividir(Ui.Grupo("Comprobante", _vista), Ui.Grupo("Resumen de pedidos por estado", _resumen), true, 68));
        Controls.Add(barra);
    }

    public override string Nombre => "Reportes y comprobantes";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.PedidoConfirmado, CambioNegocio.PedidoDespachado,
        CambioNegocio.PedidoEntregado, CambioNegocio.DevolucionProcesada, CambioNegocio.PedidoCancelado, CambioNegocio.PedidoGuardado);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var (pedidos, panel) = await UsarAsync(async sp =>
        {
            var consultas = sp.GetRequiredService<ServicioConsultas>();
            return (await consultas.PedidosEnEstadoAsync("PAGADO", "DESPACHADO", "ENTREGADO"), await consultas.PanelAsync());
        });
        var anterior = (_pedido.SelectedItem as Opcion<int>)?.Valor;
        _pedido.Items.Clear();
        foreach (var p in pedidos) _pedido.Items.Add(new Opcion<int>(p.Id, $"#{p.Id} - {p.Cliente} - {p.Estado} - {p.Total:N2} USD"));
        var elegido = _pedido.Items.Cast<Opcion<int>>().FirstOrDefault(o => o.Valor == anterior);
        if (elegido is not null) _pedido.SelectedItem = elegido; else if (_pedido.Items.Count > 0) _pedido.SelectedIndex = 0;
        Ui.Cargar(_resumen, panel.PorEstado);
    }

    private async Task EmitirAsync()
    {
        if (_pedido.SelectedItem is not Opcion<int> pedido) { Informar("No hay pedidos pagados."); return; }
        var medio = ((Opcion<string>)_medio.SelectedItem!).Valor;
        var esVenta = (string)_tipo.SelectedItem! == "Venta";

        var salida = await UsarAsync(sp =>
        {
            var servicio = sp.GetRequiredService<ServicioComprobantes>();
            return esVenta ? servicio.EmitirVentaAsync(pedido.Valor, medio) : servicio.EmitirDevolucionAsync(pedido.Valor, medio);
        });
        _vista.Text = salida.Contenido;
        _ultimaRuta = salida.Ruta;
        _ruta.Text = salida.Ruta is null ? "Mostrado en pantalla." : $"Guardado en {salida.Ruta}";
    }

    private void AbrirCarpeta()
    {
        if (_ultimaRuta is null) { Informar("Primero emita un comprobante en Archivo TXT."); return; }
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{_ultimaRuta}\"") { UseShellExecute = true });
    }
}
