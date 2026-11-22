using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Inventario;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Inventario;

// mueve existencias por SKU final y deja el rastro en movimientos de inventario
public sealed class ServicioInventario
{
    private readonly IProductoRepository _productos;
    private readonly IMovimientoInventarioRepository _movimientos;

    public ServicioInventario(IProductoRepository productos, IMovimientoInventarioRepository movimientos)
    {
        _productos = productos;
        _movimientos = movimientos;
    }

    public async Task DescontarPorVentaAsync(Pedido pedido, DateTime fecha)
    {
        foreach (var (producto, cantidad) in await CargarAsync(pedido))
        {
            producto.DescontarStock(cantidad);
            await _movimientos.AgregarAsync(new MovimientoInventario(producto.Id, -cantidad, producto.Stock,
                TipoMovimiento.SalidaVenta, $"Venta del pedido #{pedido.Id}", $"PED-{pedido.Id}", fecha, pedido.Id));
        }
    }

    public async Task ReintegrarPorDevolucionAsync(Pedido pedido, string referencia, DateTime fecha)
    {
        foreach (var (producto, cantidad) in await CargarAsync(pedido))
        {
            producto.AgregarStock(cantidad);
            await _movimientos.AgregarAsync(new MovimientoInventario(producto.Id, cantidad, producto.Stock,
                TipoMovimiento.ReingresoDevolucion, $"Devolución del pedido #{pedido.Id}", referencia, fecha, pedido.Id));
        }
    }

    private async Task<List<(ProductoSimple Producto, int Cantidad)>> CargarAsync(Pedido pedido)
    {
        var requerimiento = pedido.RequerimientoPorSku();
        var productos = (await _productos.ObtenerPorCodigosAsync(requerimiento.Keys)).ToDictionary(p => p.Codigo);

        return requerimiento
            .OrderBy(r => r.Key)
            .Select(r => (productos.TryGetValue(r.Key, out var p)
                ? p
                : throw new DomainException($"El SKU {r.Key} ya no existe en el catálogo."), r.Value))
            .ToList();
    }
}
