using LogiMarket.Domain.Pagos;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos", t => t.HasCheckConstraint("CK_Pagos_Monto", "\"Monto\" > 0"));
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Monto).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Proveedor).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ReferenciaCobro).IsRequired().HasMaxLength(60);
        builder.Property(p => p.ClaveIdempotencia).IsRequired().HasMaxLength(80);
        builder.Property(p => p.Fecha).IsRequired();

        // tres candados contra cobros dobles
        builder.HasIndex(p => p.ReferenciaCobro).IsUnique();
        builder.HasIndex(p => p.ClaveIdempotencia).IsUnique();
        builder.HasIndex(p => p.PedidoId).IsUnique();

        builder.HasOne<Pedido>().WithMany().HasForeignKey(p => p.PedidoId).OnDelete(DeleteBehavior.Restrict);
    }
}
