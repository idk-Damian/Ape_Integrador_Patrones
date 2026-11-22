using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class PedidoDetalleComponenteConfiguration : IEntityTypeConfiguration<LineaComponenteSnapshot>
{
    public void Configure(EntityTypeBuilder<LineaComponenteSnapshot> builder)
    {
        builder.ToTable("PedidoDetalleComponentes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProductoCodigo).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ProductoNombre).IsRequired().HasMaxLength(100);
        builder.Property(c => c.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Cantidad).IsRequired();
        builder.Property(c => c.Orden).IsRequired();
        builder.Property(c => c.PesoKg).HasColumnType("decimal(18,3)");
    }
}
