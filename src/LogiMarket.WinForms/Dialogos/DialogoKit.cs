using System.ComponentModel;
using LogiMarket.Application.Catalogo;
using LogiMarket.WinForms.Comun;

namespace LogiMarket.WinForms.Dialogos;

// sirve para crear un kit (codigo, nombre y componentes) o para agregar un componente a un kit existente
internal sealed class DialogoKit : DialogoBase
{
    private readonly bool _soloComponente;
    private readonly TextBox _codigo = Ui.Texto(160, "KIT_NUEVO");
    private readonly TextBox _nombre = Ui.Texto(240);
    private readonly ComboBox _componente = Ui.Combo(220);
    private readonly NumericUpDown _cantidad = Ui.Numero(1, 500, 0, 70);
    private readonly BindingList<ComponenteSolicitado> _lista = new();
    private readonly DataGridView _grilla = Ui.Grilla();

    public DialogoKit(IEnumerable<string> codigosDisponibles, string? kitDestino = null)
        : base(kitDestino is null ? "Nuevo kit" : $"Agregar componente a {kitDestino}")
    {
        _soloComponente = kitDestino is not null;
        _componente.Items.AddRange(codigosDisponibles.Cast<object>().ToArray());
        if (_componente.Items.Count > 0) _componente.SelectedIndex = 0;
        _cantidad.Value = 1;

        var contenido = new Panel { Height = _soloComponente ? 90 : 330, Width = 520 };
        var datos = _soloComponente
            ? Ui.Formulario(("Componente", _componente), ("Cantidad", _cantidad))
            : Ui.Formulario(("Código", _codigo), ("Nombre", _nombre), ("Componente", _componente), ("Cantidad", _cantidad));
        contenido.Controls.Add(datos);

        if (!_soloComponente)
        {
            _grilla.DataSource = _lista;
            _grilla.Dock = DockStyle.None;
            _grilla.SetBounds(8, 150, 500, 130);
            var agregar = Ui.Boton("Agregar a la lista", (_, _) => AgregarALista(), 150);
            var quitar = Ui.Boton("Quitar", (_, _) => { if (_grilla.CurrentRow?.DataBoundItem is ComponenteSolicitado c) _lista.Remove(c); }, 90);
            agregar.Location = new Point(8, 285);
            quitar.Location = new Point(170, 285);
            contenido.Controls.AddRange(new Control[] { _grilla, agregar, quitar });
        }
        Montar(contenido);
    }

    public string Codigo => _codigo.Text.Trim().ToUpperInvariant();
    public string NombreKit => _nombre.Text.Trim();
    public IReadOnlyList<ComponenteSolicitado> Componentes => _lista.ToList();
    public ComponenteSolicitado ComponenteElegido => new((string)_componente.SelectedItem!, (int)_cantidad.Value);

    private void AgregarALista()
    {
        if (_componente.SelectedItem is not string codigo) return;
        var existente = _lista.FirstOrDefault(c => c.Codigo == codigo);
        if (existente is not null) _lista.Remove(existente);
        _lista.Add(new ComponenteSolicitado(codigo, (int)_cantidad.Value + (existente?.Cantidad ?? 0)));
    }

    protected override bool Validar()
    {
        if (_soloComponente) return _componente.SelectedItem is not null;
        bool listaOk = _lista.Count > 0;
        Errores.SetError(_grilla, listaOk ? "" : "Agregue al menos un componente.");
        return Requerido(_codigo, _codigo.Text, "Ingrese el código.") & Requerido(_nombre, _nombre.Text, "Ingrese el nombre.") & listaOk;
    }
}
