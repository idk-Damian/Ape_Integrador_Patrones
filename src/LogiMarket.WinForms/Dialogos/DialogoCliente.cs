using LogiMarket.Application.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Comun;

namespace LogiMarket.WinForms.Dialogos;

internal sealed class DialogoCliente : DialogoBase
{
    private readonly int _id;
    private readonly TextBox _nombre = Ui.Texto(260);
    private readonly TextBox _email = Ui.Texto(260, "nombre@dominio.com");
    private readonly TextBox _telefono = Ui.Texto(140, "09xxxxxxxx");
    private readonly ComboBox _categoria = Ui.Combo(140);
    private readonly CheckBox _activo = new() { Text = "Cliente activo", Checked = true, AutoSize = true };

    public DialogoCliente(ClienteDto? existente) : base(existente is null ? "Nuevo cliente" : $"Editar {existente.Nombre}")
    {
        _categoria.Items.AddRange(new object[] { CategoriaCliente.Normal, CategoriaCliente.Vip });
        _categoria.SelectedIndex = 0;
        if (existente is not null)
        {
            _id = existente.Id;
            _nombre.Text = existente.Nombre;
            _email.Text = existente.Email;
            _telefono.Text = existente.Telefono;
            _categoria.SelectedItem = existente.Categoria;
            _activo.Checked = existente.Activo;
        }
        _activo.Enabled = existente is not null;

        Montar(Ui.Formulario(("Nombre", _nombre), ("Email", _email), ("Teléfono", _telefono), ("Categoría", _categoria), ("Estado", _activo)));
    }

    public ClienteDto Resultado => new(_id, _nombre.Text.Trim(), _email.Text.Trim(), _telefono.Text.Trim(),
        (CategoriaCliente)_categoria.SelectedItem!, _activo.Checked);

    protected override bool Validar()
    {
        bool emailOk = _email.Text.Contains('@');
        Errores.SetError(_email, emailOk ? "" : "Ingrese un email válido.");
        return Requerido(_nombre, _nombre.Text, "Ingrese el nombre.") & emailOk
               & Requerido(_telefono, _telefono.Text, "Ingrese el teléfono.");
    }
}
