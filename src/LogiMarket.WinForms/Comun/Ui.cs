using System.ComponentModel;
using System.Text.RegularExpressions;

namespace LogiMarket.WinForms.Comun;

// fabrica de controles con el mismo aspecto en todas las pantallas
internal static class Ui
{
    public static readonly Font Fuente = new("Segoe UI", 9.5f);
    public static readonly Font FuenteTitulo = new("Segoe UI Semibold", 12f);
    public static readonly Font FuenteMonoespaciada = new("Consolas", 10f);

    public static Label Etiqueta(string texto, bool negrita = false) => new()
    {
        Text = texto,
        AutoSize = true,
        Margin = new Padding(3, 8, 3, 3),
        Font = negrita ? new Font(Fuente, FontStyle.Bold) : Fuente
    };

    public static TextBox Texto(int ancho = 160, string? ayuda = null) => new()
    {
        Width = ancho,
        PlaceholderText = ayuda ?? "",
        Margin = new Padding(3, 4, 3, 3)
    };

    public static Button Boton(string texto, EventHandler alPulsar, int ancho = 0)
    {
        var boton = new Button
        {
            Text = texto,
            AutoSize = ancho == 0,
            Width = ancho == 0 ? 90 : ancho,
            Height = 30,
            Margin = new Padding(3),
            UseVisualStyleBackColor = true
        };
        boton.Click += alPulsar;
        return boton;
    }

    public static ComboBox Combo(int ancho = 160) => new()
    {
        Width = ancho,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Margin = new Padding(3, 4, 3, 3)
    };

    public static NumericUpDown Numero(decimal minimo, decimal maximo, int decimales = 0, int ancho = 80) => new()
    {
        Minimum = minimo,
        Maximum = maximo,
        DecimalPlaces = decimales,
        Width = ancho,
        Increment = decimales == 0 ? 1 : 0.01m,
        ThousandsSeparator = decimales > 0,
        Margin = new Padding(3, 4, 3, 3)
    };

    public static FlowLayoutPanel Barra(params Control[] controles)
    {
        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(0, 2, 0, 6)
        };
        Agregar(barra, controles);
        return barra;
    }

    public static GroupBox Grupo(string titulo, Control contenido, DockStyle dock = DockStyle.Fill)
    {
        var grupo = new GroupBox { Text = titulo, Dock = dock, Padding = new Padding(8), Font = Fuente };
        contenido.Dock = DockStyle.Fill;
        grupo.Controls.Add(contenido);
        return grupo;
    }

    // dos columnas: etiqueta y control; el orden de tabulacion sigue el orden de las filas
    public static TableLayoutPanel Formulario(params (string Etiqueta, Control Control)[] filas)
    {
        var tabla = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(4)
        };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        int tab = 0;
        foreach (var (texto, control) in filas)
        {
            var etiqueta = Etiqueta(texto);
            etiqueta.TabIndex = tab++;
            control.TabIndex = tab++;
            tabla.Controls.Add(etiqueta);
            tabla.Controls.Add(control);
        }
        return tabla;
    }

    // divide el espacio en dos partes por porcentaje (sin SplitContainer para evitar medidas invalidas al iniciar)
    public static TableLayoutPanel Dividir(Control primero, Control segundo, bool horizontal, float porcentajePrimero)
    {
        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = horizontal ? 2 : 1, RowCount = horizontal ? 1 : 2 };
        if (horizontal)
        {
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, porcentajePrimero));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - porcentajePrimero));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        }
        else
        {
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, porcentajePrimero));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100 - porcentajePrimero));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        }
        primero.Dock = DockStyle.Fill;
        segundo.Dock = DockStyle.Fill;
        tabla.Controls.Add(primero, 0, 0);
        tabla.Controls.Add(segundo, horizontal ? 1 : 0, horizontal ? 0 : 1);
        return tabla;
    }

    public static void Agregar(Control contenedor, params Control[] controles)
    {
        int tab = contenedor.Controls.Count;
        foreach (var c in controles)
        {
            c.TabIndex = tab++;
            contenedor.Controls.Add(c);
        }
    }

    public static DataGridView Grilla()
    {
        var grilla = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            StandardTab = true,
            Font = Fuente
        };
        grilla.DataBindingComplete += (_, _) => DarFormato(grilla);
        return grilla;
    }

    // conserva la fila seleccionada (por el valor de la primera columna) al recargar los datos
    public static void Cargar<T>(DataGridView grilla, IEnumerable<T> datos)
    {
        var clave = grilla.CurrentRow?.Cells.Count > 0 ? grilla.CurrentRow.Cells[0].Value : null;
        grilla.DataSource = new BindingList<T>(datos.ToList());
        if (clave is null) return;
        foreach (DataGridViewRow fila in grilla.Rows)
        {
            if (Equals(fila.Cells[0].Value, clave))
            {
                grilla.CurrentCell = fila.Cells[0];
                break;
            }
        }
    }

    public static T? Seleccionado<T>(DataGridView grilla) where T : class
        => grilla.CurrentRow?.DataBoundItem as T;

    private static void DarFormato(DataGridView grilla)
    {
        foreach (DataGridViewColumn columna in grilla.Columns)
        {
            columna.HeaderText = Separar(columna.DataPropertyName);
            var tipo = Nullable.GetUnderlyingType(columna.ValueType ?? typeof(object)) ?? columna.ValueType;
            if (tipo == typeof(decimal))
            {
                columna.DefaultCellStyle.Format = "N2";
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else if (tipo == typeof(int) || tipo == typeof(long))
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            else if (tipo == typeof(DateTime))
                columna.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }
    }

    private static readonly Dictionary<string, string> Encabezados = new()
    {
        ["Sku"] = "SKU", ["Id"] = "N.º", ["PesoKg"] = "Peso (kg)", ["PedidoId"] = "Pedido", ["Email"] = "Email"
    };

    public static string Separar(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return nombre;
        if (Encabezados.TryGetValue(nombre, out var fijo)) return fijo;
        var texto = Regex.Replace(nombre, "(?<=[a-zñ0-9])(?=[A-Z])", " ");
        return char.ToUpper(texto[0]) + texto[1..].ToLowerInvariant();
    }
}
