using LogiMarket.Domain.Despachos;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class DespachoConfiguration : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> builder)
    {
        builder.ToTable("Despachos");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.PedidoId).IsRequired();
        builder.Property(d => d.Direccion).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Fecha).IsRequired();

        builder.HasIndex(d => d.PedidoId);

        builder.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(d => d.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
