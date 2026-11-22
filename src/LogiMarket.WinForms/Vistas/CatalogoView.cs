using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using LogiMarket.WinForms.Dialogos;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 03: productos (CRUD) y kits anidados con desglose por Visitor e Iterator
internal sealed class CatalogoView : VistaBase
{
    private readonly TextBox _buscar = Ui.Texto(220, "SKU o nombre");
    private readonly DataGridView _productos = Ui.Grilla();
    private readonly DataGridView _kits = Ui.Grilla();
    private readonly DataGridView _desglose = Ui.Grilla();
    private readonly Label _resumenDesglose = new() { Dock = DockStyle.Top, Height = 64, Padding = new Padding(4) };

    public CatalogoView(IServiceProvider servicios) : base(servicios)
    {
        var barraProductos = Ui.Barra(Ui.Etiqueta("Buscar"), _buscar,
            Ui.Boton("Buscar", async (_, _) => await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.CatalogoModificado))),
            Ui.Boton("Nuevo producto", async (_, _) => await EjecutarAsync(NuevoProductoAsync)),
            Ui.Boton("Editar producto", async (_, _) => await EjecutarAsync(EditarProductoAsync)));
        _buscar.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.CatalogoModificado)); };
        _productos.CellDoubleClick += async (_, _) => await EjecutarAsync(EditarProductoAsync);

        var barraKits = Ui.Barra(
            Ui.Boton("Nuevo kit", async (_, _) => await EjecutarAsync(NuevoKitAsync)),
            Ui.Boton("Agregar componente", async (_, _) => await EjecutarAsync(AgregarComponenteAsync)),
            Ui.Boton("Ver desglose", async (_, _) => await EjecutarAsync(DesglosarAsync)));
        _kits.SelectionChanged += async (_, _) => { if (_kits.Focused) await EjecutarAsync(DesglosarAsync); };

        var panelProductos = new Panel { Dock = DockStyle.Fill };
        panelProductos.Controls.Add(Ui.Grupo("Productos", _productos));
        panelProductos.Controls.Add(barraProductos);

        var panelDesglose = new Panel { Dock = DockStyle.Fill };
        panelDesglose.Controls.Add(_desglose);
        panelDesglose.Controls.Add(_resumenDesglose);

        var panelKits = new Panel { Dock = DockStyle.Fill };
        panelKits.Controls.Add(Ui.Grupo("Kits", _kits));
        panelKits.Controls.Add(barraKits);
        var inferior = Ui.Dividir(panelKits, Ui.Grupo("Desglose físico del kit seleccionado", panelDesglose), true, 50);

        Controls.Add(Ui.Dividir(panelProductos, inferior, false, 50));
    }

    public override string Nombre => "Catálogo y kits";

    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.CatalogoModificado, CambioNegocio.PedidoConfirmado,
        CambioNegocio.DevolucionProcesada);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var texto = _buscar.Text;
        var (productos, kits) = await UsarAsync(async sp =>
        {
            var servicio = sp.GetRequiredService<ServicioCatalogo>();
            return (await servicio.ListarProductosAsync(texto), await servicio.ListarKitsAsync());
        });
        Ui.Cargar(_productos, productos);
        Ui.Cargar(_kits, kits);
    }

    private async Task NuevoProductoAsync()
    {
        using var dialogo = new DialogoProducto(null);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().CrearProductoAsync(dialogo.Resultado));
        MostrarResultado(r);
        if (r.Exito) await AvisarYRecargarAsync();
    }

    private async Task EditarProductoAsync()
    {
        if (Ui.Seleccionado<ProductoDto>(_productos) is not { } producto) { Informar("Seleccione un producto."); return; }
        using var dialogo = new DialogoProducto(producto);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().EditarProductoAsync(dialogo.Resultado));
        MostrarResultado(r);
        if (r.Exito) await AvisarYRecargarAsync();
    }

    private async Task NuevoKitAsync()
    {
        using var dialogo = new DialogoKit(await CodigosAsync());
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().CrearKitAsync(dialogo.Codigo, dialogo.NombreKit, dialogo.Componentes));
        MostrarResultado(r);
        if (r.Exito) await AvisarYRecargarAsync();
    }

    private async Task AgregarComponenteAsync()
    {
        if (Ui.Seleccionado<KitDto>(_kits) is not { } kit) { Informar("Seleccione un kit."); return; }
        using var dialogo = new DialogoKit(await CodigosAsync(), kit.Codigo);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().AgregarAKitAsync(kit.Codigo, dialogo.ComponenteElegido));
        MostrarResultado(r);
        if (r.Exito) await AvisarYRecargarAsync();
    }

    private async Task DesglosarAsync()
    {
        if (Ui.Seleccionado<KitDto>(_kits) is not { } kit) return;
        var d = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().DesglosarAsync(kit.Codigo));
        Ui.Cargar(_desglose, d.PorSku);
        _resumenDesglose.Text =
            $"{d.Codigo}: precio {d.Precio:N2} USD, peso logístico {d.PesoKg:N2} kg\n" +
            $"Visita anidada: {string.Join(" > ", d.KitsVisitados)}\n" +
            $"Recorrido del iterador: {string.Join(", ", d.RecorridoIterador.Select(c => $"{c.Codigo} x{c.Cantidad}"))}";
    }

    private async Task<IReadOnlyList<string>> CodigosAsync()
        => await UsarAsync(async sp =>
        {
            var servicio = sp.GetRequiredService<ServicioCatalogo>();
            return (IReadOnlyList<string>)(await servicio.ListarProductosAsync()).Select(p => p.Sku)
                .Concat((await servicio.ListarKitsAsync()).Select(k => k.Codigo)).ToList();
        });

    private async Task AvisarYRecargarAsync()
    {
        await CargarDatosAsync(CambioNegocio.CatalogoModificado);
        await AvisarAsync(CambioNegocio.CatalogoModificado);
    }
}
