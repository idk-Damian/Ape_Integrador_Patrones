using LogiMarket.Application.Comun;
using LogiMarket.Application.Consultas.Filtros;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Presentacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Comun;

// cada pantalla es un colega del mediador: usa casos de uso de Application y nunca llama a otra pantalla
internal abstract class VistaBase : UserControl, IColegaPantalla
{
    protected VistaBase(IServiceProvider servicios)
    {
        Servicios = servicios;
        Mediador = servicios.GetRequiredService<IMediadorPantallas>();
        Usuario = servicios.GetRequiredService<IContextoUsuario>();
        Dock = DockStyle.Fill;
        Padding = new Padding(10);
        Font = Ui.Fuente;
    }

    protected IServiceProvider Servicios { get; }
    protected IMediadorPantallas Mediador { get; }
    protected IContextoUsuario Usuario { get; }

    public abstract string Nombre { get; }
    public abstract IReadOnlySet<CambioNegocio> Intereses { get; }

    public async Task RefrescarAsync(CambioNegocio cambio)
    {
        try
        {
            await CargarDatosAsync(cambio);
        }
        catch (Exception ex) when (EsEsperada(ex))
        {
            // una pantalla de solo lectura no debe romper el refresco de las demas
        }
    }

    protected abstract Task CargarDatosAsync(CambioNegocio cambio);

    protected T Unico<T>() where T : notnull => Servicios.GetRequiredService<T>();

    // cada operacion usa su propio alcance (y su propio DbContext)
    protected async Task<T> UsarAsync<T>(Func<IServiceProvider, Task<T>> accion)
    {
        using var alcance = Servicios.CreateScope();
        return await accion(alcance.ServiceProvider);
    }

    protected async Task EjecutarAsync(Func<Task> accion)
    {
        UseWaitCursor = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    protected Task AvisarAsync(CambioNegocio cambio) => Mediador.NotificarAsync(this, cambio);

    protected static void MostrarResultado(ResultadoOperacion resultado)
        => MessageBox.Show(resultado.Mensaje, resultado.Exito ? "Operación realizada" : "Operación rechazada",
            MessageBoxButtons.OK, resultado.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

    protected static void Informar(string mensaje, string titulo = "LogiMarket 360")
        => MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

    protected static bool Confirmar(string mensaje, string titulo = "Confirmar")
        => MessageBox.Show(mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
           == DialogResult.Yes;

    protected static void MostrarError(Exception ex)
    {
        if (EsEsperada(ex))
            MessageBox.Show(ex.Message, ex is AccesoDenegadoException ? "Acceso denegado" : "No se pudo completar",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        else
            MessageBox.Show($"Ocurrió un error inesperado.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static bool EsEsperada(Exception ex)
        => EjecutorOperaciones.EsEsperada(ex) || ex is FiltroInvalidoException;

    protected static HashSet<CambioNegocio> Cambios(params CambioNegocio[] cambios)
    {
        var conjunto = cambios.ToHashSet();
        conjunto.Add(CambioNegocio.SesionIniciada);
        conjunto.Add(CambioNegocio.DatosRestablecidos);
        return conjunto;
    }

    protected static Label Titulo(string texto) => new()
    {
        Text = texto,
        Font = Ui.FuenteTitulo,
        Dock = DockStyle.Top,
        Height = 32
    };
}
