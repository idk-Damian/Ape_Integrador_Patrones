using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Entregas;

// fabrica abstracta: cada familia entrega tarifa, plazo y etiqueta compatibles entre si
public interface IFabricaEntrega
{
    TipoEntrega Tipo { get; }
    ITarifaEntrega CrearTarifa();
    IPlazoEntrega CrearPlazo();
    IEtiquetaEntrega CrearEtiqueta();
}

public interface ITarifaEntrega
{
    TipoEntrega Familia { get; }
    decimal Calcular();
}

public interface IPlazoEntrega
{
    TipoEntrega Familia { get; }
    int Dias { get; }
    DateTime EstimarEntrega(DateTime desde);
}

public interface IEtiquetaEntrega
{
    TipoEntrega Familia { get; }
    string GenerarNumeroGuia(int pedidoId, DateTime fecha);
    string Construir(string numeroGuia, string destinatario, DateTime fechaEstimada);
}
