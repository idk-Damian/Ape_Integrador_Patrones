using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class PedidoDetalleConfiguration : IEntityTypeConfiguration<LineaPedido>
{
    public void Configure(EntityTypeBuilder<LineaPedido> builder)
    {
        builder.ToTable("PedidoDetalles");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.PedidoId).IsRequired();
        builder.Property(l => l.ElementoCodigo).IsRequired().HasMaxLength(20);
        builder.Property(l => l.ElementoNombre).IsRequired().HasMaxLength(100);
        builder.Property(l => l.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Cantidad).IsRequired();
        builder.Property(l => l.EsKit).IsRequired();

        // Propiedades computadas
        builder.Ignore(l => l.Subtotal);
        builder.Ignore(l => l.TotalUnidadesFisicas);

        // Navegación componentes snapshot con backing field
        builder.HasMany(l => l.Componentes)
            .WithOne()
            .HasForeignKey(c => c.LineaPedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Componentes)
            .HasField("_componentes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
