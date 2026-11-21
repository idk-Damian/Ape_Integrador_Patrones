using LogiMarket.Domain.Devoluciones;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("Devoluciones");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.PedidoId).IsRequired();
        // Devolución única por pedido (UNIQUE)
        builder.HasIndex(d => d.PedidoId).IsUnique();

        builder.Property(d => d.Motivo).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Monto).HasColumnType("decimal(18,2)");
        builder.Property(d => d.Fecha).IsRequired();

        builder.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(d => d.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
