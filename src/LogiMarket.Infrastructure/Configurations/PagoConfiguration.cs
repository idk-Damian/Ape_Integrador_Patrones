using LogiMarket.Domain.Pagos;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PedidoId).IsRequired();
        builder.Property(p => p.Monto).HasColumnType("decimal(18,2)");

        builder.Property(p => p.ReferenciaCobro).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.ReferenciaCobro).IsUnique();

        builder.Property(p => p.ClaveIdempotencia).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.ClaveIdempotencia).IsUnique();

        builder.Property(p => p.Fecha).IsRequired();

        builder.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
