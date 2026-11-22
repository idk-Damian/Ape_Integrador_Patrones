using LogiMarket.Application;
using LogiMarket.Application.Mantenimiento;
using LogiMarket.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms;

internal static class Program
{
    // composition root: el unico lugar que conoce Infrastructure
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var carpetaDatos = Path.Combine(AppContext.BaseDirectory, "datos");
        Directory.CreateDirectory(carpetaDatos);

        System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        System.Windows.Forms.Application.ThreadException += (_, e) => Registrar(carpetaDatos, e.Exception);

        var servicios = new ServiceCollection();
        servicios.AgregarInfraestructura($"Data Source={Path.Combine(carpetaDatos, "logimarket.db")}", carpetaDatos);
        servicios.AgregarAplicacion();

        using var proveedor = servicios.BuildServiceProvider();
        try
        {
            // crea la base desde cero (migraciones + semilla) si aun no existe
            proveedor.GetRequiredService<IMantenimientoBaseDatos>().InicializarAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Registrar(carpetaDatos, ex);
            return;
        }

        while (true)
        {
            using var acceso = new FormAcceso(proveedor);
            if (acceso.ShowDialog() != DialogResult.OK) break;

            using var principal = new FormPrincipal(proveedor);
            System.Windows.Forms.Application.Run(principal);
            if (!principal.CerrarSesionSolicitado) break;
        }
    }

    private static void Registrar(string carpeta, Exception ex)
    {
        File.AppendAllText(Path.Combine(carpeta, "errores.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ex}{Environment.NewLine}");
        MessageBox.Show($"Ocurrió un error inesperado.\n\n{ex.Message}\n\nDetalle en datos\\errores.log", "LogiMarket 360",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
