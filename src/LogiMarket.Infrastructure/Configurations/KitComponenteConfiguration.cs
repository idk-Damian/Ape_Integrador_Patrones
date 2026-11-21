using LogiMarket.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class KitComponenteConfiguration : IEntityTypeConfiguration<ComponenteKit>
{
    public void Configure(EntityTypeBuilder<ComponenteKit> builder)
    {
        builder.ToTable("KitComponentes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.KitPadreId).IsRequired();
        builder.Property(c => c.CodigoComponente).IsRequired().HasMaxLength(20);
        builder.Property(c => c.TipoComponente).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Cantidad).IsRequired();

        builder.Ignore(c => c.Elemento);
    }
}
