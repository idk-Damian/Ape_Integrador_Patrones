using LogiMarket.Application.Excepciones;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Seguridad;

// matriz rol-operacion; la UI puede ocultar botones, pero quien decide es esta clase
public static class PoliticaPermisos
{
    private static readonly Dictionary<RolUsuario, HashSet<Operacion>> Permisos = new()
    {
        [RolUsuario.Admin] = Enum.GetValues<Operacion>().ToHashSet(),
        [RolUsuario.Ventas] = new()
        {
            Operacion.ConsultarInformacion, Operacion.GestionarClientes, Operacion.GestionarPedidos,
            Operacion.ValidarPedido, Operacion.CobrarPedido, Operacion.CancelarPedido, Operacion.ProcesarDevolucion
        },
        [RolUsuario.Bodega] = new()
        {
            Operacion.ConsultarInformacion, Operacion.DespacharPedido, Operacion.EntregarPedido
        },
        [RolUsuario.Consulta] = new() { Operacion.ConsultarInformacion }
    };

    public static bool Permite(RolUsuario? rol, Operacion operacion)
        => rol is not null && Permisos[rol.Value].Contains(operacion);

    public static void Exigir(IContextoUsuario usuario, Operacion operacion)
    {
        if (!usuario.Autenticado)
            throw new AccesoDenegadoException("Debe iniciar sesión.");
        if (!Permite(usuario.Rol, operacion))
            throw new AccesoDenegadoException(
                $"El rol {usuario.Rol} del usuario '{usuario.NombreUsuario}' no puede {Describir(operacion)}.");
    }

    private static string Describir(Operacion operacion) => operacion switch
    {
        Operacion.CobrarPedido => "cobrar pedidos",
        Operacion.DespacharPedido => "despachar pedidos",
        Operacion.EntregarPedido => "registrar entregas",
        Operacion.ProcesarDevolucion => "procesar devoluciones",
        Operacion.GestionarCatalogo => "modificar el catálogo",
        _ => $"realizar la operación {operacion}"
    };
}
