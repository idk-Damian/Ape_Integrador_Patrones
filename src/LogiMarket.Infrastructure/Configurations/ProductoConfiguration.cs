using LogiMarket.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<ProductoSimple>
{
    public void Configure(EntityTypeBuilder<ProductoSimple> builder)
    {
        builder.ToTable("Productos", t =>
        {
            t.HasCheckConstraint("CK_Productos_Precio", "\"PrecioUnitario\" > 0");
            t.HasCheckConstraint("CK_Productos_Stock", "\"Stock\" >= 0");
            t.HasCheckConstraint("CK_Productos_Peso", "\"PesoKg\" >= 0");
        });
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.HasIndex(p => p.Nombre);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Categoria).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PesoKg).HasColumnType("decimal(18,3)");
        builder.Property(p => p.Stock).IsRequired();

        // concurrencia optimista: si dos ventas leen el mismo stock solo la primera guarda
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.Ignore(p => p.EsKit);
    }
}
