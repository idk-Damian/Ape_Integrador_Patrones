using LogiMarket.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class KitComponenteConfiguration : IEntityTypeConfiguration<ComponenteKit>
{
    public void Configure(EntityTypeBuilder<ComponenteKit> builder)
    {
        builder.ToTable("KitComponentes", t =>
        {
            t.HasCheckConstraint("CK_KitComponentes_Cantidad", "\"Cantidad\" > 0");
            // exactamente uno de los dos debe estar lleno
            t.HasCheckConstraint("CK_KitComponentes_Destino",
                "(\"ProductoId\" IS NULL AND \"SubKitId\" IS NOT NULL) OR (\"ProductoId\" IS NOT NULL AND \"SubKitId\" IS NULL)");
        });
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Cantidad).IsRequired();
        builder.Property(c => c.Orden).IsRequired();

        builder.HasOne(c => c.Producto)
            .WithMany()
            .HasForeignKey(c => c.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SubKit)
            .WithMany()
            .HasForeignKey(c => c.SubKitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.KitPadreId, c.ProductoId }).IsUnique();
        builder.HasIndex(c => new { c.KitPadreId, c.SubKitId }).IsUnique();

        builder.Ignore(c => c.Elemento);
        builder.Ignore(c => c.CodigoComponente);
    }
}
