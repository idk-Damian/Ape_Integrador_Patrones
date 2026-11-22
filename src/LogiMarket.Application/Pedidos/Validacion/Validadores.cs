using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Pedidos.Validacion;

public sealed class ValidadorClienteActivo : ValidadorPedido
{
    public override string Nombre => "Cliente activo";

    protected override string? Revisar(ContextoValidacion c)
        => c.Cliente switch
        {
            null => "El pedido no tiene un cliente registrado.",
            { Activo: false } => $"El cliente {c.Cliente.Nombre} está inactivo.",
            _ => null
        };
}

public sealed class ValidadorLineas : ValidadorPedido
{
    public const int MaximoPorLinea = 500;

    public override string Nombre => "Líneas y cantidades";

    protected override string? Revisar(ContextoValidacion c)
    {
        if (c.Pedido.Lineas.Count == 0) return "El pedido no tiene líneas.";
        var mala = c.Pedido.Lineas.FirstOrDefault(l => l.Cantidad <= 0 || l.Cantidad > MaximoPorLinea);
        if (mala is not null) return $"La cantidad de {mala.ElementoCodigo} debe estar entre 1 y {MaximoPorLinea}.";
        var vacia = c.Pedido.Lineas.FirstOrDefault(l => l.Componentes.Count == 0);
        return vacia is null ? null : $"La línea {vacia.ElementoCodigo} no tiene componentes físicos.";
    }
}

public sealed class ValidadorStock : ValidadorPedido
{
    public override string Nombre => "Stock disponible";

    protected override string? Revisar(ContextoValidacion c)
    {
        var faltantes = new List<string>();
        foreach (var (sku, requerido) in c.Pedido.RequerimientoPorSku())
        {
            if (!c.Productos.TryGetValue(sku, out var producto))
                faltantes.Add($"{sku} no existe");
            else if (producto.Stock < requerido)
                faltantes.Add($"{sku} requiere {requerido} y hay {producto.Stock}");
        }
        return faltantes.Count == 0 ? null : "Stock insuficiente: " + string.Join("; ", faltantes) + ".";
    }
}

public sealed class ValidadorAutorizacion : ValidadorPedido
{
    public override string Nombre => "Autorización y estado";

    protected override string? Revisar(ContextoValidacion c)
    {
        if (!PoliticaPermisos.Permite(c.Usuario.Rol, c.Operacion))
            return $"El usuario '{c.Usuario.NombreUsuario}' no tiene permiso para {c.Operacion}.";

        var permitidos = c.Operacion == Operacion.CobrarPedido
            ? new[] { "BORRADOR", "VALIDADO" }
            : new[] { "BORRADOR" };

        return permitidos.Contains(c.Pedido.EstadoNombre)
            ? null
            : $"Un pedido en estado {c.Pedido.EstadoNombre} no admite {c.Operacion}.";
    }
}
