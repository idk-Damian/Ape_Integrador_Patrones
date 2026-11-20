using LogiMarket.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<ProductoSimple>
{
    public void Configure(EntityTypeBuilder<ProductoSimple> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(10);
        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Stock).IsRequired();
        builder.Property(p => p.PesoKg).HasColumnType("decimal(18,4)");

        // Concurrencia optimista
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.Ignore(p => p.EsKit);
    }
}
