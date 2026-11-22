using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Inventario;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario", t =>
        {
            t.HasCheckConstraint("CK_Movimientos_Cantidad", "\"Cantidad\" <> 0");
            t.HasCheckConstraint("CK_Movimientos_StockResultante", "\"StockResultante\" >= 0");
        });
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Referencia).IsRequired().HasMaxLength(80);
        builder.Property(m => m.Fecha).IsRequired();

        builder.HasIndex(m => m.ProductoId);
        builder.HasIndex(m => m.PedidoId);
        // un pedido descuenta cada SKU una sola vez por tipo de movimiento
        builder.HasIndex(m => new { m.PedidoId, m.ProductoId, m.Tipo }).IsUnique();

        builder.HasOne<ProductoSimple>().WithMany().HasForeignKey(m => m.ProductoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Pedido>().WithMany().HasForeignKey(m => m.PedidoId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    }
}
