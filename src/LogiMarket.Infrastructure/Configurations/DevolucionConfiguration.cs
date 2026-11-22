using LogiMarket.Domain.Devoluciones;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("Devoluciones", t => t.HasCheckConstraint("CK_Devoluciones_Monto", "\"MontoReembolso\" > 0"));
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Motivo).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.MontoReembolso).HasColumnType("decimal(18,2)");
        builder.Property(d => d.ReferenciaReembolso).IsRequired().HasMaxLength(60);
        builder.Property(d => d.Fecha).IsRequired();

        // devolucion unica por pedido y reembolso unico
        builder.HasIndex(d => d.PedidoId).IsUnique();
        builder.HasIndex(d => d.ReferenciaReembolso).IsUnique();

        builder.HasOne<Pedido>().WithMany().HasForeignKey(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);
    }
}
