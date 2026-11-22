using LogiMarket.WinForms.Comun;

namespace LogiMarket.WinForms.Dialogos;

// ventana modal con Aceptar/Cancelar y validacion junto al campo (ErrorProvider)
internal abstract class DialogoBase : Form
{
    protected readonly ErrorProvider Errores = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly FlowLayoutPanel _botones;

    protected DialogoBase(string titulo)
    {
        Text = titulo;
        Font = Ui.Fuente;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(12);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        var aceptar = Ui.Boton("Aceptar", (_, _) => { if (Validar()) DialogResult = DialogResult.OK; }, 100);
        var cancelar = Ui.Boton("Cancelar", (_, _) => DialogResult = DialogResult.Cancel, 100);
        _botones = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(0, 8, 0, 0)
        };
        _botones.Controls.Add(cancelar);
        _botones.Controls.Add(aceptar);
        AcceptButton = aceptar;
        CancelButton = cancelar;
    }

    protected void Montar(Control contenido)
    {
        contenido.Dock = DockStyle.Top;
        Controls.Add(_botones);
        Controls.Add(contenido);
        _botones.TabIndex = 100;
        var preferido = contenido.GetPreferredSize(Size.Empty);
        ClientSize = new Size(
            Math.Max(preferido.Width, contenido.Width) + Padding.Horizontal + 20,
            Math.Max(preferido.Height, contenido.Height) + 56 + Padding.Vertical);
    }

    protected abstract bool Validar();

    protected bool Requerido(Control control, string valor, string mensaje)
    {
        bool ok = !string.IsNullOrWhiteSpace(valor);
        Errores.SetError(control, ok ? "" : mensaje);
        return ok;
    }
}
