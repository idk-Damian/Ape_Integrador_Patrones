using LogiMarket.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class KitConfiguration : IEntityTypeConfiguration<KitCompuesto>
{
    public void Configure(EntityTypeBuilder<KitCompuesto> builder)
    {
        builder.ToTable("Kits");
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Codigo).IsRequired().HasMaxLength(20);
        builder.HasIndex(k => k.Codigo).IsUnique();
        builder.Property(k => k.Nombre).IsRequired().HasMaxLength(100);

        builder.Ignore(k => k.PrecioUnitario);
        builder.Ignore(k => k.EsKit);

        builder.HasMany(k => k.Componentes)
            .WithOne()
            .HasForeignKey(c => c.KitPadreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(k => k.Componentes)
            .HasField("_componentes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
