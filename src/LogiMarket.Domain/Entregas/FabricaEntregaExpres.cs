using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Entregas;

public sealed class FabricaEntregaExpres : IFabricaEntrega
{
    public TipoEntrega Tipo => TipoEntrega.Expres;
    public ITarifaEntrega CrearTarifa() => new TarifaExpres();
    public IPlazoEntrega CrearPlazo() => new PlazoExpres();
    public IEtiquetaEntrega CrearEtiqueta() => new EtiquetaExpres();

    private sealed class TarifaExpres : ITarifaEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Expres;
        public decimal Calcular() => 12.00m;
    }

    private sealed class PlazoExpres : IPlazoEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Expres;
        public int Dias => 1;
        public DateTime EstimarEntrega(DateTime desde) => desde.AddDays(Dias);
    }

    private sealed class EtiquetaExpres : IEtiquetaEntrega
    {
        public TipoEntrega Familia => TipoEntrega.Expres;

        public string GenerarNumeroGuia(int pedidoId, DateTime fecha)
            => $"EXP-{fecha:yyyyMMdd}-{pedidoId:D6}";

        public string Construir(string numeroGuia, string destinatario, DateTime fechaEstimada)
            => $"ENVIO EXPRES PRIORITARIO | Guia {numeroGuia} | Para: {destinatario} | Entrega: {fechaEstimada:dd/MM/yyyy}";
    }
}
