using LogiMarket.Application.Comun;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Inventario;

namespace LogiMarket.Application.Catalogo;

public sealed record ProductoDto(string Sku, string Nombre, string Categoria, string Descripcion, decimal Precio, int Stock, decimal PesoKg);

public sealed record KitDto(string Codigo, string Nombre, decimal Precio, string Composicion, decimal PesoKg);

public sealed record ComponenteSolicitado(string Codigo, int Cantidad);

public sealed record DesgloseKit(string Codigo, decimal Precio, IReadOnlyList<ComponenteFisico> PorSku,
    IReadOnlyList<ComponenteFisico> RecorridoIterador, IReadOnlyList<string> KitsVisitados, decimal PesoKg);

public sealed class ServicioCatalogo
{
    private readonly IProductoRepository _productos;
    private readonly IKitRepository _kits;
    private readonly IMovimientoInventarioRepository _movimientos;
    private readonly ResolutorCatalogo _resolutor;
    private readonly IUnitOfWork _uow;
    private readonly IContextoUsuario _usuario;
    private readonly IReloj _reloj;
    private readonly EjecutorOperaciones _ejecutor;

    public ServicioCatalogo(IProductoRepository productos, IKitRepository kits, IMovimientoInventarioRepository movimientos,
        ResolutorCatalogo resolutor, IUnitOfWork uow, IContextoUsuario usuario, IReloj reloj, EjecutorOperaciones ejecutor)
    {
        _productos = productos;
        _kits = kits;
        _movimientos = movimientos;
        _resolutor = resolutor;
        _uow = uow;
        _usuario = usuario;
        _reloj = reloj;
        _ejecutor = ejecutor;
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarProductosAsync(string? texto = null)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        return (await _productos.ListarAsync(texto))
            .Select(p => new ProductoDto(p.Codigo, p.Nombre, p.Categoria, p.Descripcion, p.PrecioUnitario, p.Stock, p.PesoKg))
            .ToList();
    }

    public async Task<IReadOnlyList<KitDto>> ListarKitsAsync()
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        return (await _kits.ListarAsync()).Select(k =>
        {
            var peso = new VisitantePesoLogistico();
            k.Aceptar(peso);
            var composicion = string.Join(" + ", k.Componentes.Select(c => $"{c.Cantidad} x {c.CodigoComponente}"));
            return new KitDto(k.Codigo, k.Nombre, k.PrecioUnitario, composicion, peso.PesoTotalKg);
        }).ToList();
    }

    public Task<ResultadoOperacion> CrearProductoAsync(ProductoDto dto)
        => _ejecutor.EjecutarAsync("Crear producto", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.GestionarCatalogo);
            if (await _productos.ExisteCodigoAsync(dto.Sku) || await _kits.ExisteCodigoAsync(dto.Sku))
                throw new ConflictoDuplicadoException($"El código {dto.Sku} ya existe en el catálogo.");

            var producto = new ProductoSimple(dto.Sku, dto.Nombre, dto.Precio, dto.Stock, dto.PesoKg, dto.Categoria, dto.Descripcion);
            await _productos.AgregarAsync(producto);
            await _uow.GuardarCambiosAsync();

            if (producto.Stock > 0)
            {
                await _movimientos.AgregarAsync(new MovimientoInventario(producto.Id, producto.Stock, producto.Stock,
                    TipoMovimiento.InventarioInicial, "Alta de producto", $"ALTA-{producto.Codigo}", _reloj.Ahora));
                await _uow.GuardarCambiosAsync();
            }
            return $"Producto {producto.Codigo} creado.";
        });

    public Task<ResultadoOperacion> EditarProductoAsync(ProductoDto dto)
        => _ejecutor.EjecutarAsync("Editar producto", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.GestionarCatalogo);
            var producto = await _productos.ObtenerPorCodigoAsync(dto.Sku)
                ?? throw new RecursoNoEncontradoException($"No existe el producto {dto.Sku}.");
            if (dto.Stock < 0) throw new Domain.Exceptions.DomainException("El stock no puede ser negativo.");

            producto.ActualizarDatos(dto.Nombre, dto.Categoria, dto.Descripcion, dto.PesoKg);
            producto.ActualizarPrecio(dto.Precio);

            // un cambio de stock desde el catalogo queda como ajuste trazable
            int diferencia = dto.Stock - producto.Stock;
            if (diferencia != 0)
            {
                if (diferencia > 0) producto.AgregarStock(diferencia); else producto.DescontarStock(-diferencia);
                await _movimientos.AgregarAsync(new MovimientoInventario(producto.Id, diferencia, producto.Stock,
                    diferencia > 0 ? TipoMovimiento.AjusteEntrada : TipoMovimiento.AjusteSalida,
                    "Ajuste manual desde catálogo", $"AJUSTE-{_usuario.NombreUsuario}", _reloj.Ahora));
            }
            await _uow.GuardarCambiosAsync();
            return $"Producto {producto.Codigo} actualizado.";
        });

    public Task<ResultadoOperacion> CrearKitAsync(string codigo, string nombre, IReadOnlyList<ComponenteSolicitado> componentes)
        => _ejecutor.EjecutarAsync("Crear kit", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.GestionarCatalogo);
            if (componentes.Count == 0) throw new Domain.Exceptions.DomainException("Un kit necesita al menos un componente.");
            if (await _productos.ExisteCodigoAsync(codigo) || await _kits.ExisteCodigoAsync(codigo))
                throw new ConflictoDuplicadoException($"El código {codigo} ya existe en el catálogo.");

            var kit = new KitCompuesto(codigo, nombre);
            foreach (var c in componentes)
                kit.AgregarComponente(await _resolutor.ObtenerAsync(c.Codigo), c.Cantidad);

            await _kits.AgregarAsync(kit);
            await _uow.GuardarCambiosAsync();
            return $"Kit {kit.Codigo} creado con precio {kit.PrecioUnitario:N2} USD.";
        });

    public Task<ResultadoOperacion> AgregarAKitAsync(string codigoKit, ComponenteSolicitado componente)
        => _ejecutor.EjecutarAsync("Agregar componente a kit", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.GestionarCatalogo);
            var kit = await _kits.ObtenerPorCodigoAsync(codigoKit)
                ?? throw new RecursoNoEncontradoException($"No existe el kit {codigoKit}.");
            kit.AgregarComponente(await _resolutor.ObtenerAsync(componente.Codigo), componente.Cantidad);
            await _uow.GuardarCambiosAsync();
            return $"{componente.Codigo} agregado a {kit.Codigo}. Nuevo precio {kit.PrecioUnitario:N2} USD.";
        });

    public async Task<DesgloseKit> DesglosarAsync(string codigo)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        var kit = await _kits.ObtenerPorCodigoAsync(codigo) ?? throw new RecursoNoEncontradoException($"No existe el kit {codigo}.");

        var desglose = new VisitanteDesgloseFisico();
        var peso = new VisitantePesoLogistico();
        kit.Aceptar(desglose);
        kit.Aceptar(peso);

        var recorrido = new List<ComponenteFisico>();
        using (var iterador = kit.CrearIterador())
            while (iterador.MoveNext()) recorrido.Add(iterador.Current);

        return new DesgloseKit(kit.Codigo, kit.PrecioUnitario, desglose.Lineas, recorrido, desglose.KitsVisitados, peso.PesoTotalKg);
    }
}
