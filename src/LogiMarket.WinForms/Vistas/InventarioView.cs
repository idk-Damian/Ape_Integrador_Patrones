using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 10: existencias y movimientos con signo, motivo y referencia
internal sealed class InventarioView : VistaBase
{
    private readonly DataGridView _existencias = Ui.Grilla();
    private readonly DataGridView _movimientos = Ui.Grilla();
    private readonly ComboBox _sku = Ui.Combo(120);
    private IReadOnlyList<MovimientoDto> _todos = Array.Empty<MovimientoDto>();

    public InventarioView(IServiceProvider servicios) : base(servicios)
    {
        _sku.SelectedIndexChanged += (_, _) => Filtrar();
        var movimientos = new Panel { Dock = DockStyle.Fill };
        movimientos.Controls.Add(Ui.Grupo("Movimientos de inventario", _movimientos));
        movimientos.Controls.Add(Ui.Barra(Ui.Etiqueta("SKU"), _sku));
        Controls.Add(Ui.Dividir(Ui.Grupo("Existencias actuales", _existencias), movimientos, true, 38));
    }

    public override string Nombre => "Inventario y movimientos";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.CatalogoModificado, CambioNegocio.PedidoConfirmado,
        CambioNegocio.DevolucionProcesada);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var (productos, movimientos) = await UsarAsync(async sp =>
            (await sp.GetRequiredService<ServicioCatalogo>().ListarProductosAsync(), await sp.GetRequiredService<ServicioConsultas>().MovimientosAsync()));
        Ui.Cargar(_existencias, productos.Select(p => new { p.Sku, p.Nombre, p.Stock, Alerta = p.Stock <= ServicioConsultas.UmbralStockBajo ? "Stock bajo" : "" }).ToList());
        _todos = movimientos;

        var elegido = _sku.SelectedItem as string;
        _sku.Items.Clear();
        _sku.Items.Add("(todos)");
        foreach (var p in productos) _sku.Items.Add(p.Sku);
        _sku.SelectedItem = elegido is not null && _sku.Items.Contains(elegido) ? elegido : "(todos)";
        Filtrar();
    }

    private void Filtrar()
    {
        var sku = _sku.SelectedItem as string;
        Ui.Cargar(_movimientos, sku is null or "(todos)" ? _todos : _todos.Where(m => m.Sku == sku).ToList());
    }
}
