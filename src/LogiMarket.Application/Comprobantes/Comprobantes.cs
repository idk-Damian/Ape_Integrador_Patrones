namespace LogiMarket.Application.Comprobantes;

public sealed record DocumentoTexto(string Titulo, string NombreArchivo, IReadOnlyList<string> Lineas)
{
    public string Contenido => string.Join(Environment.NewLine, Lineas);
}

public sealed record ResultadoSalida(string Medio, string Contenido, string? Ruta);

// implementador del bridge: a donde va el documento
public interface ISalidaDocumento
{
    string Medio { get; }
    ResultadoSalida Emitir(DocumentoTexto documento);
}

public static class MediosSalida
{
    public const string VistaPrevia = "VISTA_PREVIA";
    public const string ArchivoTxt = "ARCHIVO_TXT";
}

// abstraccion del bridge: que documento se arma; no sabe si termina en pantalla o en archivo
public abstract class Comprobante
{
    private readonly ISalidaDocumento _salida;

    protected Comprobante(ISalidaDocumento salida) => _salida = salida;

    public ResultadoSalida Emitir() => _salida.Emitir(Componer());

    protected abstract DocumentoTexto Componer();

    protected static IEnumerable<string> Encabezado(string titulo, string numero, DateTime fecha)
    {
        yield return "LOGIMARKET 360";
        yield return titulo;
        yield return "Documento interno de demostración, sin valor tributario.";
        yield return new string('-', 56);
        yield return $"Número: {numero}    Fecha: {fecha:dd/MM/yyyy HH:mm}";
    }
}

public sealed record LineaComprobante(string Codigo, string Descripcion, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public sealed record DatosVenta(int PedidoId, string Cliente, string Email, DateTime Fecha, IReadOnlyList<LineaComprobante> Lineas,
    decimal Subtotal, string Politica, decimal Descuento, string Entrega, decimal Tarifa, decimal Total, string ReferenciaCobro);

public sealed record DatosDevolucion(int PedidoId, string Cliente, DateTime Fecha, string Motivo, int DiasDesdeEntrega,
    decimal TotalPagado, decimal TarifaNoReembolsable, decimal Reembolso, string ReferenciaReembolso);

public sealed class ComprobanteVenta : Comprobante
{
    private readonly DatosVenta _d;

    public ComprobanteVenta(ISalidaDocumento salida, DatosVenta datos) : base(salida) => _d = datos;

    protected override DocumentoTexto Componer()
    {
        var lineas = Encabezado("COMPROBANTE DE VENTA", $"V-{_d.PedidoId:D6}", _d.Fecha).ToList();
        lineas.Add($"Cliente: {_d.Cliente} <{_d.Email}>");
        lineas.Add(new string('-', 56));
        lineas.AddRange(_d.Lineas.Select(l =>
            $"{l.Codigo,-16} {l.Cantidad,4} x {l.PrecioUnitario,8:N2} = {l.Subtotal,9:N2}  {l.Descripcion}"));
        lineas.Add(new string('-', 56));
        lineas.Add($"Subtotal:                 {_d.Subtotal,10:N2}");
        lineas.Add($"Descuento ({_d.Politica}):  {-_d.Descuento,10:N2}");
        lineas.Add($"Entrega {_d.Entrega}:        {_d.Tarifa,10:N2}");
        lineas.Add($"TOTAL USD:                {_d.Total,10:N2}");
        lineas.Add($"Referencia de cobro: {_d.ReferenciaCobro}");
        return new DocumentoTexto("Comprobante de venta", $"venta_{_d.PedidoId:D6}.txt", lineas);
    }
}

public sealed class ComprobanteDevolucion : Comprobante
{
    private readonly DatosDevolucion _d;

    public ComprobanteDevolucion(ISalidaDocumento salida, DatosDevolucion datos) : base(salida) => _d = datos;

    protected override DocumentoTexto Componer()
    {
        var lineas = Encabezado("COMPROBANTE DE DEVOLUCIÓN", $"D-{_d.PedidoId:D6}", _d.Fecha).ToList();
        lineas.Add($"Cliente: {_d.Cliente}");
        lineas.Add($"Pedido original: #{_d.PedidoId}   Motivo: {_d.Motivo}   Días desde la entrega: {_d.DiasDesdeEntrega}");
        lineas.Add(new string('-', 56));
        lineas.Add($"Total pagado:             {_d.TotalPagado,10:N2}");
        lineas.Add($"Envío (no reembolsable):  {-_d.TarifaNoReembolsable,10:N2}");
        lineas.Add($"REEMBOLSO USD:            {_d.Reembolso,10:N2}");
        lineas.Add($"Referencia de reembolso: {_d.ReferenciaReembolso}");
        return new DocumentoTexto("Comprobante de devolución", $"devolucion_{_d.PedidoId:D6}.txt", lineas);
    }
}

// salida a pantalla: entrega el texto para que la vista lo muestre
public sealed class SalidaVistaPrevia : ISalidaDocumento
{
    public string Medio => MediosSalida.VistaPrevia;

    public ResultadoSalida Emitir(DocumentoTexto documento) => new(Medio, documento.Contenido, null);
}
