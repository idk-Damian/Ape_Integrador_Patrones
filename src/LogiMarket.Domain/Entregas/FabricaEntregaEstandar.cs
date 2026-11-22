using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Entregas;

public sealed class FabricaEntregaEstandar : IFabricaEntrega
{
    public TipoEntrega Tipo => TipoEntrega.Estandar;
    public ITarifaEntrega CrearTarifa() => new TarifaEstandar();
    public IPlazoEntrega CrearPlazo() => new PlazoEstandar();
    public IEtiquetaEntrega CrearEtiqueta() => new EtiquetaEstandar();

    // los productos son internos para que nadie los combine con otra familia por fuera
    private sealed class TarifaEstandar : ITarifaEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Estandar;
        public decimal Calcular() => 5.00m;
    }

    private sealed class PlazoEstandar : IPlazoEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Estandar;
        public int Dias => 3;
        public DateTime EstimarEntrega(DateTime desde) => desde.AddDays(Dias);
    }

    private sealed class EtiquetaEstandar : IEtiquetaEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Estandar;

        public string GenerarNumeroGuia(int pedidoId, DateTime fecha)
            => $"STD-{fecha:yyyyMMdd}-{pedidoId:D6}";

        public string Construir(string numeroGuia, string destinatario, DateTime fechaEstimada)
            => $"ENVIO ESTANDAR | Guia {numeroGuia} | Para: {destinatario} | Llega aprox.: {fechaEstimada:dd/MM/yyyy}";
    }
}
