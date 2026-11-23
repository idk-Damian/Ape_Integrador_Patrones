using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Descuentos;

namespace LogiMarket.Tests.Soporte;

// catalogo oficial de la guia armado en memoria para las pruebas de dominio
internal sealed class Datos
{
    // fecha fija para las pruebas de dominio; el pedido ya no toma la hora del sistema
    public static readonly DateTime Hoy = new(2026, 11, 2, 10, 0, 0);

    public ProductoSimple P01 { get; } = new("P01", "Mouse USB", 20.00m, 8, 0.20m);
    public ProductoSimple P02 { get; } = new("P02", "Teclado", 30.00m, 5, 0.70m);
    public ProductoSimple P03 { get; } = new("P03", "Memoria USB", 10.00m, 20, 0.10m);
    public ProductoSimple P04 { get; } = new("P04", "Webcam", 40.00m, 4, 0.35m);
    public ProductoSimple P05 { get; } = new("P05", "Alfombrilla", 5.00m, 30, 0.10m);
    public KitCompuesto KitOficina { get; }
    public KitCompuesto KitTeletrabajo { get; }

    public Datos()
    {
        KitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        KitOficina.AgregarComponente(P01, 1);
        KitOficina.AgregarComponente(P02, 1);

        KitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
        KitTeletrabajo.AgregarComponente(KitOficina, 1);
        KitTeletrabajo.AgregarComponente(P03, 1);
    }

    public static CalculadoraDescuento Calculadora()
        => new(new IEstrategiaDescuento[] { new Mayorista15(), new Vip10(), new SinDescuento() });

    public static PedidoBuilder Builder() => new PedidoBuilder(Calculadora()).EnFecha(Hoy);

    public Pedido Borrador(CategoriaCliente categoria = CategoriaCliente.Normal)
        => Builder().ParaCliente(1, categoria).ConLinea(P01, 1).ConEntrega(new FabricaEntregaEstandar()).Construir();
}
