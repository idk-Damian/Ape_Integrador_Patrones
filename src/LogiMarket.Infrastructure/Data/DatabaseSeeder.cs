using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Data;

/// <summary>
/// Seeder idempotente de base de datos para LogiMarket 360.
/// Garantiza la creación de productos, kits, clientes y usuarios semilla
/// sin duplicar registros al ejecutarse múltiples veces.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(LogiMarketDbContext context)
    {
        // 1. Semilla de Productos
        var p01 = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "P01");
        if (p01 is null)
        {
            p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
            await context.Productos.AddAsync(p01);
        }

        var p02 = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "P02");
        if (p02 is null)
        {
            p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.60m);
            await context.Productos.AddAsync(p02);
        }

        var p03 = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "P03");
        if (p03 is null)
        {
            p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.30m);
            await context.Productos.AddAsync(p03);
        }

        var p04 = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "P04");
        if (p04 is null)
        {
            p04 = new ProductoSimple("P04", "Webcam", 40.00m, 4, 0.25m);
            await context.Productos.AddAsync(p04);
        }

        var p05 = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "P05");
        if (p05 is null)
        {
            p05 = new ProductoSimple("P05", "Alfombrilla", 5.00m, 30, 0.05m);
            await context.Productos.AddAsync(p05);
        }

        await context.SaveChangesAsync();

        // 2. Semilla de Kits
        // KIT_OFICINA = P01 + P02 ($50.00)
        var kitOficina = await context.Kits
            .Include(k => k.Componentes)
            .FirstOrDefaultAsync(k => k.Codigo == "KIT_OFICINA");

        if (kitOficina is null)
        {
            kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
            kitOficina.AgregarComponente(p01, 1);
            kitOficina.AgregarComponente(p02, 1);
            await context.Kits.AddAsync(kitOficina);
            await context.SaveChangesAsync();
        }

        // KIT_TELETRABAJO = KIT_OFICINA + P03 ($60.00, Peso exacto: 0.10 + 0.60 + 0.30 = 1.00 kg)
        var kitTeletrabajo = await context.Kits
            .Include(k => k.Componentes)
            .FirstOrDefaultAsync(k => k.Codigo == "KIT_TELETRABAJO");

        if (kitTeletrabajo is null)
        {
            kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
            kitTeletrabajo.AgregarComponente(kitOficina, 1);
            kitTeletrabajo.AgregarComponente(p03, 1);
            await context.Kits.AddAsync(kitTeletrabajo);
            await context.SaveChangesAsync();
        }

        // 3. Semilla de Usuarios con contraseña hasheada (PBKDF2)
        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "admin"))
        {
            await context.Usuarios.AddAsync(new Usuario("admin", "Admin123!", RolUsuario.Admin));
        }

        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "ventas"))
        {
            await context.Usuarios.AddAsync(new Usuario("ventas", "Ventas123!", RolUsuario.Ventas));
        }

        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "bodega"))
        {
            await context.Usuarios.AddAsync(new Usuario("bodega", "Bodega123!", RolUsuario.Bodega));
        }

        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "consulta"))
        {
            await context.Usuarios.AddAsync(new Usuario("consulta", "Consulta123!", RolUsuario.Consulta));
        }

        // 4. Clientes demo
        if (!await context.Clientes.AnyAsync(c => c.Email == "ana@logimarket.com"))
        {
            await context.Clientes.AddAsync(new Cliente("Ana López", "ana@logimarket.com", CategoriaCliente.Vip));
        }

        if (!await context.Clientes.AnyAsync(c => c.Email == "luis@logimarket.com"))
        {
            await context.Clientes.AddAsync(new Cliente("Luis Pérez", "luis@logimarket.com", CategoriaCliente.Normal));
        }

        if (!await context.Clientes.AnyAsync(c => c.Email == "carmen@logimarket.com"))
        {
            await context.Clientes.AddAsync(new Cliente("Carmen Gómez", "carmen@logimarket.com", CategoriaCliente.Vip));
        }

        await context.SaveChangesAsync();
    }
}
