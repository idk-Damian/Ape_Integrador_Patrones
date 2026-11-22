using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Presentacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.WinForms.Comun;
using LogiMarket.WinForms.Vistas;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms;

// ventana principal: navegacion lateral y una sola pantalla visible a la vez;
// todas las pantallas se registran en el mediador para refrescarse sin conocerse entre si
internal sealed class FormPrincipal : Form
{
    private readonly IServiceProvider _servicios;
    private readonly IMediadorPantallas _mediador;
    private readonly List<VistaBase> _vistas = new();
    private readonly Panel _contenido = new() { Dock = DockStyle.Fill };
    private readonly Label _encabezado = new() { Dock = DockStyle.Top, Height = 40, Font = Ui.FuenteTitulo, Padding = new Padding(12, 8, 0, 0) };
    private readonly FlowLayoutPanel _menu = new()
    {
        Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(8)
    };
    private Button? _activo;

    public FormPrincipal(IServiceProvider servicios)
    {
        _servicios = servicios;
        _mediador = servicios.GetRequiredService<IMediadorPantallas>();
        var sesion = servicios.GetRequiredService<IContextoUsuario>();

        Text = $"LogiMarket 360 - {sesion.NombreUsuario} ({sesion.Rol})";
        Font = Ui.Fuente;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1320, 820);
        MinimumSize = new Size(1080, 680);
        WindowState = FormWindowState.Maximized;

        _vistas.AddRange(new VistaBase[]
        {
            new PanelGeneralView(servicios), new CatalogoView(servicios), new ClientesView(servicios),
            new CarritoView(servicios), new PedidosView(servicios), new CobroView(servicios),
            new DespachoView(servicios), new DevolucionesView(servicios), new InventarioView(servicios),
            new ReportesView(servicios), new AuditoriaView(servicios)
        });

        foreach (var vista in _vistas)
        {
            var vistaActual = vista;
            var boton = new Button
            {
                Text = vista.Nombre, Width = 226, Height = 36, TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 4), UseVisualStyleBackColor = true
            };
            boton.Click += (_, _) => Mostrar(vistaActual, boton);
            _menu.Controls.Add(boton);
            vista.Visible = false;
            _contenido.Controls.Add(vista);
            _mediador.Registrar(vista);
        }

        var usuario = new Label
        {
            Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(10, 4, 4, 4), ForeColor = SystemColors.GrayText,
            Text = $"Sesión: {sesion.NombreUsuario}\nRol: {sesion.Rol}"
        };
        var cerrar = new Button { Text = "Cerrar sesión", Dock = DockStyle.Bottom, Height = 34, UseVisualStyleBackColor = true };
        cerrar.Click += (_, _) => CerrarSesion();

        var lateral = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = SystemColors.ControlLight };
        lateral.Controls.Add(_menu);
        lateral.Controls.Add(usuario);
        lateral.Controls.Add(cerrar);

        Controls.Add(_contenido);
        Controls.Add(_encabezado);
        Controls.Add(lateral);

        Load += async (_, _) =>
        {
            foreach (var vista in _vistas) await vista.RefrescarAsync(CambioNegocio.SesionIniciada);
            Mostrar(_vistas[0], (Button)_menu.Controls[0]);
        };
        FormClosed += (_, _) => { foreach (var vista in _vistas) _mediador.Desregistrar(vista); };
    }

    public bool CerrarSesionSolicitado { get; private set; }

    private void Mostrar(VistaBase vista, Button boton)
    {
        foreach (var v in _vistas) v.Visible = ReferenceEquals(v, vista);
        if (_activo is not null) _activo.Font = Ui.Fuente;
        boton.Font = new Font(Ui.Fuente, FontStyle.Bold);
        _activo = boton;
        _encabezado.Text = vista.Nombre;
    }

    private void CerrarSesion()
    {
        var carrito = _servicios.GetRequiredService<ServicioCarrito>();
        if (carrito.TieneLineas && MessageBox.Show("Hay un carrito sin guardar. ¿Cerrar sesión de todos modos?", "Cerrar sesión",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;

        carrito.Reiniciar();
        using (var alcance = _servicios.CreateScope())
            alcance.ServiceProvider.GetRequiredService<ServicioAutenticacion>().CerrarSesion();
        CerrarSesionSolicitado = true;
        Close();
    }
}
