using LogiMarket.Application.Seguridad;
using LogiMarket.WinForms.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms;

// pantalla 01: identificacion local con usuario y contrasena (se verifica contra el hash guardado)
internal sealed class FormAcceso : Form
{
    private readonly IServiceProvider _servicios;
    private readonly TextBox _usuario = Ui.Texto(220);
    private readonly TextBox _clave = Ui.Texto(220);
    private readonly Label _mensaje = new() { AutoSize = true, ForeColor = Color.Firebrick, Margin = new Padding(3, 8, 3, 3) };

    public FormAcceso(IServiceProvider servicios)
    {
        _servicios = servicios;
        Text = "LogiMarket 360 - Acceso";
        Font = Ui.Fuente;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(430, 300);
        Padding = new Padding(24);

        _clave.UseSystemPasswordChar = true;
        var ingresar = Ui.Boton("Ingresar", async (_, _) => await IngresarAsync(), 120);
        var salir = Ui.Boton("Salir", (_, _) => { DialogResult = DialogResult.Cancel; Close(); }, 120);
        AcceptButton = ingresar;
        CancelButton = salir;

        var titulo = new Label { Text = "LogiMarket 360", Font = Ui.FuenteTitulo, Dock = DockStyle.Top, Height = 34 };
        var subtitulo = new Label { Text = "Ventas y logística de accesorios de computación", Dock = DockStyle.Top, Height = 28 };
        var formulario = Ui.Formulario(("Usuario", _usuario), ("Contraseña", _clave));
        var botones = Ui.Barra(ingresar, salir, _mensaje);
        var ayuda = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            ForeColor = SystemColors.GrayText,
            Text = "Usuarios de prueba: admin, ventas, bodega y consulta.\nContraseña: nombre del rol con mayúscula inicial y 123! (ej. Ventas123!)"
        };

        Controls.Add(botones);
        Controls.Add(formulario);
        Controls.Add(subtitulo);
        Controls.Add(titulo);
        Controls.Add(ayuda);
        _usuario.TabIndex = 0;
        _clave.TabIndex = 1;
    }

    private async Task IngresarAsync()
    {
        _mensaje.Text = "";
        if (string.IsNullOrWhiteSpace(_usuario.Text) || string.IsNullOrEmpty(_clave.Text))
        {
            _mensaje.Text = "Ingrese usuario y contraseña.";
            return;
        }

        using var alcance = _servicios.CreateScope();
        var resultado = await alcance.ServiceProvider.GetRequiredService<ServicioAutenticacion>()
            .IniciarSesionAsync(_usuario.Text.Trim(), _clave.Text);
        if (!resultado.Exito)
        {
            _mensaje.Text = resultado.Mensaje;
            _clave.SelectAll();
            _clave.Focus();
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }
}
