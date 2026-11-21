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
        builder.ToTable("MovimientosInventario");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProductoId).IsRequired();
        builder.Property(m => m.Cantidad).IsRequired();
        builder.Property(m => m.Tipo).IsRequired();
        builder.Property(m => m.Fecha).IsRequired();

        builder.HasIndex(m => m.ProductoId);

        builder.HasOne<ProductoSimple>()
            .WithMany()
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(m => m.PedidoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
