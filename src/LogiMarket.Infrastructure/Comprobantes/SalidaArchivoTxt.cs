using System.Text;
using LogiMarket.Application.Comprobantes;

namespace LogiMarket.Infrastructure.Comprobantes;

// salida a disco: escribe el comprobante como .txt en la carpeta de salidas
public sealed class SalidaArchivoTxt : ISalidaDocumento
{
    private readonly string _carpeta;

    public SalidaArchivoTxt(string carpeta) => _carpeta = carpeta;

    public string Medio => MediosSalida.ArchivoTxt;

    public ResultadoSalida Emitir(DocumentoTexto documento)
    {
        Directory.CreateDirectory(_carpeta);
        var ruta = Path.Combine(_carpeta, documento.NombreArchivo);
        File.WriteAllText(ruta, documento.Contenido, new UTF8Encoding(true));
        return new ResultadoSalida(Medio, documento.Contenido, ruta);
    }
}
