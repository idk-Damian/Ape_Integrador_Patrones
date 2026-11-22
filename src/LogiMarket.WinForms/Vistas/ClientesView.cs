using LogiMarket.Application.Clientes;
using LogiMarket.Application.Presentacion;
using LogiMarket.WinForms.Comun;
using LogiMarket.WinForms.Dialogos;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Vistas;

// pantalla 04: busqueda, registro, categoria y edicion de clientes
internal sealed class ClientesView : VistaBase
{
    private readonly TextBox _buscar = Ui.Texto(240, "Nombre o email");
    private readonly DataGridView _grilla = Ui.Grilla();

    public ClientesView(IServiceProvider servicios) : base(servicios)
    {
        var barra = Ui.Barra(Ui.Etiqueta("Buscar"), _buscar,
            Ui.Boton("Buscar", async (_, _) => await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.ClientesModificados))),
            Ui.Boton("Nuevo cliente", async (_, _) => await EjecutarAsync(() => GuardarAsync(null))),
            Ui.Boton("Editar cliente", async (_, _) => await EjecutarAsync(EditarSeleccionadoAsync)));
        _buscar.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await EjecutarAsync(() => CargarDatosAsync(CambioNegocio.ClientesModificados)); };
        _grilla.CellDoubleClick += async (_, _) => { if (Ui.Seleccionado<ClienteDto>(_grilla) is { } c) await EjecutarAsync(() => GuardarAsync(c)); };

        Controls.Add(Ui.Grupo("Clientes", _grilla));
        Controls.Add(barra);
    }

    public override string Nombre => "Clientes";
    public override IReadOnlySet<CambioNegocio> Intereses => Cambios(CambioNegocio.ClientesModificados);

    protected override async Task CargarDatosAsync(CambioNegocio cambio)
    {
        var texto = _buscar.Text;
        Ui.Cargar(_grilla, await UsarAsync(sp => sp.GetRequiredService<ServicioClientes>().ListarAsync(texto)));
    }

    private Task EditarSeleccionadoAsync()
    {
        if (Ui.Seleccionado<ClienteDto>(_grilla) is { } cliente) return GuardarAsync(cliente);
        Informar("Seleccione un cliente.");
        return Task.CompletedTask;
    }

    private async Task GuardarAsync(ClienteDto? existente)
    {
        using var dialogo = new DialogoCliente(existente);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        var r = await UsarAsync(sp => sp.GetRequiredService<ServicioClientes>().GuardarAsync(dialogo.Resultado));
        MostrarResultado(r);
        if (!r.Exito) return;
        await CargarDatosAsync(CambioNegocio.ClientesModificados);
        await AvisarAsync(CambioNegocio.ClientesModificados);
    }
}
