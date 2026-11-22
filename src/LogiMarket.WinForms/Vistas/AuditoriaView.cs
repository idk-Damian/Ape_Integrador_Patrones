using System.Runtime.CompilerServices;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Comandos;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Mantenimiento;
using LogiMarket.Application.Metricas;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 12: auditoria, bitacora tecnica, outbox, comandos, mediador y panel de demostracion controlada
internal sealed class AuditoriaView : VistaBase
{
    private readonly DataGridView _auditoria = Ui.Grilla();
    private readonly DataGridView _bitacora = Ui.Grilla();
    private readonly DataGridView _eventos = Ui.Grilla();
    private readonly DataGridView _comandos = Ui.Grilla();
    private readonly DataGridView _mediacion = Ui.Grilla();
    private readonly DataGridView _flyweight = Ui.Grilla();
    private readonly ComboBox _categoria = Ui.Combo(150);
    private readonly ComboBox _proveedor = Ui.Combo(150);
    private readonly CheckBox _fallarCobros = new() { Text = "Inducir rechazo de cobros", AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
    private readonly CheckBox _fallarAvisos = new() { Text = "Inducir fallo del notificador", AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
    private readonly NumericUpDown _filas = Ui.Numero(1000, 200000, 0, 100);
    private readonly Label _medicion = new() { Dock = DockStyle.Top, Height = 92, Font = Ui.FuenteMonoespaciada, Padding = new Padding(6) };
    private readonly Label _metricas = new() { Dock = DockStyle.Top, Height = 26, Padding = new Padding(4) };

    public AuditoriaView(IServiceProvider servicios) : base(servicios)
    {
        var control = Unico<IControlSimulacion>();
        _categoria.Items.AddRange(new object[] { "(todas)", CategoriasBitacora.Pago, CategoriasBitacora.Seguridad, CategoriasBitacora.Notificacion,
            CategoriasBitacora.Outbox, CategoriasBitacora.Comando, CategoriasBitacora.Negocio });
        _categoria.SelectedIndex = 0;
        _categoria.SelectedIndexChanged += (_, _) => CargarBitacora();
        _proveedor.Items.AddRange(new object[] { ProveedoresPago.Legado, ProveedoresPago.Alternativa });
        _proveedor.SelectedItem = control.ProveedorPago;
        _proveedor.SelectedIndexChanged += (_, _) => control.ProveedorPago = (string)_proveedor.SelectedItem!;
        _fallarCobros.CheckedChanged += (_, _) => control.FallarCobros = _fallarCobros.Checked;
        _fallarAvisos.CheckedChanged += (_, _) => control.FallarNotificaciones = _fallarAvisos.Checked;
        _filas.Value = 1000;

        var pestanas = new TabControl { Dock = DockStyle.Fill };
        pestanas.TabPages.Add(Pagina("Auditoría de negocio", _auditoria));
        pestanas.TabPages.Add(Pagina("Bitácora técnica", _bitacora, Ui.Barra(Ui.Etiqueta("Categoría"), _categoria)));
        pestanas.TabPages.Add(Pagina("Eventos (Outbox)", _eventos,
            Ui.Barra(Ui.Boton("Procesar pendientes", async (_, _) => await EjecutarAsync(ProcesarOutboxAsync)))));
        pestanas.TabPages.Add(Pagina("Comandos ejecutados", _comandos));
        pestanas.TabPages.Add(Pagina("Mediador", _mediacion));
        pestanas.TabPages.Add(PaginaDemostracion());

        Controls.Add(pestanas);
        Controls.Add(_metricas);
    }

    public override string Nombre => "Auditoría y demostración";

    public override IReadOnlySet<CambioNegocio> Intereses => Enum.GetValues<CambioNegocio>().ToHashSet();

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var (auditoria, eventos) = await UsarAsync(async sp =>
        {
            var consultas = sp.GetRequiredService<ServicioConsultas>();
            return (await consultas.AuditoriaAsync(), await consultas.EventosAsync());
        });
        Ui.Cargar(_auditoria, auditoria);
        Ui.Cargar(_eventos, eventos);
        Ui.Cargar(_comandos, Unico<InvocadorComandos>().Historial.Reverse().ToList());
        Ui.Cargar(_mediacion, Unico<FormularioPrincipalMediator>().Historial.Reverse()
            .Select(m => new { m.Fecha, m.Emisor, Cambio = m.Cambio.ToString(), Receptores = string.Join(", ", m.Receptores), Outbox = m.Outbox ?? "" })
            .ToList());
        CargarBitacora();

        var metricas = Unico<MetricasSesion>();
        _metricas.Text = $"Métricas de la sesión (instancia única): pagos {metricas.PedidosPagados}, despachos {metricas.Despachos}, " +
                         $"devoluciones {metricas.Devoluciones}, avisos {metricas.Notificaciones}, eventos {metricas.EventosProcesados}";
    }

    private void CargarBitacora()
    {
        var categoria = _categoria.SelectedItem as string;
        var entradas = Unico<IBitacora>().Entradas(categoria is null or "(todas)" ? null : categoria);
        Ui.Cargar(_bitacora, entradas.Reverse().ToList());
    }

    private async Task ProcesarOutboxAsync()
    {
        var r = await Unico<IDisparadorOutbox>().ProcesarAsync();
        Informar($"Eventos procesados: {r.Procesados}. Pendientes con error: {r.Fallidos}.\n\n{string.Join("\n", r.Detalle)}", "Outbox");
        await Mediador.NotificarAsync(this, CambioNegocio.EventosProcesados);
        await CargarDatosAsync(CambioNegocio.EventosProcesados);
    }

    private async Task CompararFlyweightAsync()
    {
        var productos = await UsarAsync(sp => sp.GetRequiredService<ServicioCatalogo>().ListarProductosAsync());
        var origen = productos.Select(p => new OrigenFila(p.Sku, p.Nombre, p.Categoria, p.Descripcion, p.Stock, p.Precio)).ToList();
        int filas = (int)_filas.Value;

        var (lista, m) = ComparativaFlyweight.Medir(origen, filas);
        Ui.Cargar(_flyweight, lista.Take(1000).Select((f, i) => new
        {
            Fila = i + 1, f.Metadatos.Codigo, f.Metadatos.Nombre, f.Stock, f.Cantidad, f.PrecioVenta,
            IdMetadatos = RuntimeHelpers.GetHashCode(f.Metadatos)
        }).ToList());
        _medicion.Text =
            $"Filas generadas: {m.Filas:N0} con {origen.Count} SKU distintos (mismos datos en ambos casos)\n" +
            $"Objetos de metadatos con Flyweight: {m.ObjetosMetadatosConFlyweight:N0}   sin Flyweight: {m.ObjetosMetadatosSinFlyweight:N0}\n" +
            $"Memoria asignada con Flyweight: {m.BytesConFlyweight:N0} bytes   sin Flyweight: {m.BytesSinFlyweight:N0} bytes\n" +
            $"Ahorro: {m.Ahorro:P1}   ReferenceEquals en filas del mismo SKU: {m.MismaReferencia}";
    }

    private async Task RestablecerAsync()
    {
        if (!Confirmar("Se borrarán todos los pedidos, pagos y movimientos y la base volverá a la semilla inicial.\n¿Restablecer la base de pruebas?",
                "Restablecer datos")) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioMantenimiento>().RestablecerAsync());
        MostrarResultado(r);
        if (r.Exito) await Mediador.NotificarAsync(null, CambioNegocio.DatosRestablecidos);
    }

    private TabPage PaginaDemostracion()
    {
        var opciones = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
        Ui.Agregar(opciones, Ui.Etiqueta("Proveedor de pago"), _proveedor, _fallarCobros, _fallarAvisos,
            Ui.Boton("Procesar Outbox", async (_, _) => await EjecutarAsync(ProcesarOutboxAsync), 140),
            Ui.Boton("Restablecer base de pruebas", async (_, _) => await EjecutarAsync(RestablecerAsync), 210));
        var flyweight = Ui.Barra(Ui.Etiqueta("Filas para la comparativa Flyweight"), _filas,
            Ui.Boton("Comparar", async (_, _) => await EjecutarAsync(CompararFlyweightAsync), 110));
        var aviso = new Label
        {
            Dock = DockStyle.Top, Height = 40, ForeColor = SystemColors.GrayText, Padding = new Padding(4),
            Text = "Los fallos inducidos recorren el mismo camino que un fallo real (rechazo del proveedor o error del notificador); no alteran cifras ni saltan validaciones."
        };

        var pagina = new TabPage("Demostración") { Padding = new Padding(6) };
        pagina.Controls.Add(Ui.Grupo("Filas de catálogo (se muestran hasta 1.000)", _flyweight));
        pagina.Controls.Add(_medicion);
        pagina.Controls.Add(flyweight);
        pagina.Controls.Add(aviso);
        pagina.Controls.Add(opciones);
        return pagina;
    }

    private static TabPage Pagina(string titulo, DataGridView grilla, Control? barra = null)
    {
        var pagina = new TabPage(titulo) { Padding = new Padding(6) };
        pagina.Controls.Add(grilla);
        if (barra is not null) pagina.Controls.Add(barra);
        return pagina;
    }
}
