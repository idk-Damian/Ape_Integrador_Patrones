using LogiMarket.Application.Comun;
using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Inventario;
using LogiMarket.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Data;

// datos iniciales del caso; se puede correr varias veces sin duplicar nada
public sealed class DatabaseSeeder
{
    private readonly LogiMarketDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IReloj _reloj;

    public DatabaseSeeder(LogiMarketDbContext context, IPasswordHasher hasher, IReloj reloj)
    {
        _context = context;
        _hasher = hasher;
        _reloj = reloj;
    }

    public async Task SembrarAsync()
    {
        // misma hora que el resto de la aplicacion (R14)
        var ahora = _reloj.Ahora;

        var p01 = await AsegurarProductoAsync("P01", "Mouse USB", 20.00m, 8, 0.20m, "Periféricos", "Mouse óptico con cable USB", ahora);
        var p02 = await AsegurarProductoAsync("P02", "Teclado", 30.00m, 5, 0.70m, "Periféricos", "Teclado estándar en español", ahora);
        var p03 = await AsegurarProductoAsync("P03", "Memoria USB", 10.00m, 20, 0.10m, "Almacenamiento", "Memoria flash USB", ahora);
        await AsegurarProductoAsync("P04", "Webcam", 40.00m, 4, 0.35m, "Video", "Cámara web HD", ahora);
        await AsegurarProductoAsync("P05", "Alfombrilla", 5.00m, 30, 0.10m, "Accesorios", "Alfombrilla para mouse", ahora);

        var oficina = await _context.Kits.FirstOrDefaultAsync(k => k.Codigo == "KIT_OFICINA");
        if (oficina is null)
        {
            oficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
            oficina.AgregarComponente(p01, 1);
            oficina.AgregarComponente(p02, 1);
            await _context.Kits.AddAsync(oficina);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Kits.AnyAsync(k => k.Codigo == "KIT_TELETRABAJO"))
        {
            // si el kit oficina venia de la base, se recarga su arbol antes de anidarlo
            await _context.Entry(oficina).Collection(k => k.Componentes).LoadAsync();
            await _context.Productos.Where(p => p.Codigo == "P01" || p.Codigo == "P02").LoadAsync();

            var teletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
            teletrabajo.AgregarComponente(oficina, 1);
            teletrabajo.AgregarComponente(p03, 1);
            await _context.Kits.AddAsync(teletrabajo);
        }

        await AsegurarUsuarioAsync("admin", "Admin123!", RolUsuario.Admin);
        await AsegurarUsuarioAsync("ventas", "Ventas123!", RolUsuario.Ventas);
        await AsegurarUsuarioAsync("bodega", "Bodega123!", RolUsuario.Bodega);
        await AsegurarUsuarioAsync("consulta", "Consulta123!", RolUsuario.Consulta);

        // datos ficticios; Carmen es normal para mostrar que MAYORISTA no depende de la categoria
        await AsegurarClienteAsync("Ana Torres", "ana@demo.local", "0990000001", CategoriaCliente.Vip);
        await AsegurarClienteAsync("Luis Mena", "luis@demo.local", "0990000002", CategoriaCliente.Normal);
        await AsegurarClienteAsync("Carmen Ruiz", "carmen@demo.local", "0990000003", CategoriaCliente.Normal);

        await _context.SaveChangesAsync();
    }

    private async Task<ProductoSimple> AsegurarProductoAsync(string sku, string nombre, decimal precio, int stock,
        decimal peso, string categoria, string descripcion, DateTime ahora)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Codigo == sku);
        if (producto is not null) return producto;

        producto = new ProductoSimple(sku, nombre, precio, stock, peso, categoria, descripcion);
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();

        await _context.MovimientosInventario.AddAsync(new MovimientoInventario(
            producto.Id, stock, stock, TipoMovimiento.InventarioInicial, "Carga inicial del catálogo", "SEED", ahora));
        return producto;
    }

    private async Task AsegurarUsuarioAsync(string nombre, string password, RolUsuario rol)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombre)) return;
        var (hash, sal) = _hasher.Generar(password);
        await _context.Usuarios.AddAsync(new Usuario(nombre, hash, sal, rol));
    }

    private async Task AsegurarClienteAsync(string nombre, string email, string telefono, CategoriaCliente categoria)
    {
        if (await _context.Clientes.AnyAsync(c => c.Email == email)) return;
        await _context.Clientes.AddAsync(new Cliente(nombre, email, telefono, categoria));
    }
}
