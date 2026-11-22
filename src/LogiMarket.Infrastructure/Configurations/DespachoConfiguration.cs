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

        builder.Property(d => d.NumeroGuia).IsRequired().HasMaxLength(40);
        builder.Property(d => d.TipoEntrega).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.FechaDespacho).IsRequired();
        builder.Property(d => d.FechaEstimada).IsRequired();

        builder.HasIndex(d => d.PedidoId).IsUnique();
        builder.HasIndex(d => d.NumeroGuia).IsUnique();

        builder.HasOne<Pedido>().WithMany().HasForeignKey(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);
    }
}
