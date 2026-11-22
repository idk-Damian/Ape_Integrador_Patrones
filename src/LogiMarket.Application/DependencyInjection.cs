using LogiMarket.Application.Comandos;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Devoluciones;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Metricas;
using LogiMarket.Application.Notificaciones;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Entregas;
using LogiMarket.Application.Inventario;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Application.Pedidos.Validacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Pedidos.Descuentos;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddSingleton<SesionUsuario>();
        services.AddSingleton<IContextoUsuario>(sp => sp.GetRequiredService<SesionUsuario>());

        // familias de entrega y politicas de descuento: agregar una nueva es registrar una linea mas
        services.AddSingleton<IFabricaEntrega, FabricaEntregaEstandar>();
        services.AddSingleton<IFabricaEntrega, FabricaEntregaExpres>();
        services.AddSingleton<CatalogoFabricasEntrega>();
        services.AddSingleton<IEstrategiaDescuento, Mayorista15>();
        services.AddSingleton<IEstrategiaDescuento, Vip10>();
        services.AddSingleton<IEstrategiaDescuento, SinDescuento>();
        services.AddSingleton<CalculadoraDescuento>();

        services.AddScoped<EjecutorOperaciones>();
        services.AddScoped<ResolutorCatalogo>();
        services.AddScoped<ServicioInventario>();
        services.AddScoped<ServicioValidacionPedidos>();
        services.AddScoped<ServicioAutenticacion>();
        services.AddScoped<ServicioPedidos>();
        services.AddScoped<ConfirmacionPedidoFacade>();
        services.AddScoped<ServicioLogistica>();

        services.AddSingleton<InvocadorComandos>();
        services.AddSingleton<MetricasSesion>();
        services.AddSingleton<ConfiguracionNotificaciones>();

        // observer + outbox: el orden de registro es el orden de aviso
        services.AddScoped<SelectorCanalNotificacion>();
        services.AddScoped<IObservadorEventos, ObservadorNotificaciones>();
        services.AddScoped<IObservadorEventos, ObservadorAuditoria>();
        services.AddScoped<IObservadorEventos, ObservadorMetricas>();
        services.AddScoped<PublicadorEventos>();
        services.AddScoped<ProcesadorOutbox>();

        // template method: una variante por motivo de devolucion
        services.AddScoped<RecursosDevolucion>();
        services.AddScoped<ProcesadorDevolucionBase, DevolucionPorDefecto>();
        services.AddScoped<ProcesadorDevolucionBase, DevolucionPorArrepentimiento>();
        services.AddScoped<ServicioDevoluciones>();

        services.AddSingleton<ISalidaDocumento, SalidaVistaPrevia>();
        services.AddScoped<ServicioComprobantes>();
        services.AddScoped<ServicioConsultasPedidos>();
        return services;
    }
}
