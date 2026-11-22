using LogiMarket.Application.Catalogo;
using LogiMarket.WinForms.Comun;

namespace LogiMarket.WinForms.Dialogos;

internal sealed class DialogoProducto : DialogoBase
{
    private readonly TextBox _sku = Ui.Texto(120, "P06");
    private readonly TextBox _nombre = Ui.Texto(260);
    private readonly TextBox _categoria = Ui.Texto(180);
    private readonly TextBox _descripcion = Ui.Texto(260);
    private readonly NumericUpDown _precio = Ui.Numero(0.01m, 100000m, 2, 110);
    private readonly NumericUpDown _stock = Ui.Numero(0, 100000, 0, 110);
    private readonly NumericUpDown _peso = Ui.Numero(0, 1000, 3, 110);

    public DialogoProducto(ProductoDto? existente) : base(existente is null ? "Nuevo producto" : $"Editar {existente.Sku}")
    {
        if (existente is not null)
        {
            _sku.Text = existente.Sku;
            _sku.ReadOnly = true;
            _nombre.Text = existente.Nombre;
            _categoria.Text = existente.Categoria;
            _descripcion.Text = existente.Descripcion;
            _precio.Value = existente.Precio;
            _stock.Value = existente.Stock;
            _peso.Value = existente.PesoKg;
        }
        else
        {
            _categoria.Text = "Accesorios";
        }

        Montar(Ui.Formulario(("SKU", _sku), ("Nombre", _nombre), ("Categoría", _categoria), ("Descripción", _descripcion),
            ("Precio (USD)", _precio), ("Stock", _stock), ("Peso (kg)", _peso)));
    }

    public ProductoDto Resultado => new(_sku.Text.Trim().ToUpperInvariant(), _nombre.Text.Trim(), _categoria.Text.Trim(),
        _descripcion.Text.Trim(), _precio.Value, (int)_stock.Value, _peso.Value);

    protected override bool Validar()
        => Requerido(_sku, _sku.Text, "Ingrese el SKU.")
           & Requerido(_nombre, _nombre.Text, "Ingrese el nombre.")
           & Requerido(_categoria, _categoria.Text, "Ingrese la categoría.");
}
