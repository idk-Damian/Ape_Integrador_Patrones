using LogiMarket.Domain.Pedidos.Historial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class CambioEstadoPedidoConfiguration : IEntityTypeConfiguration<CambioEstadoPedido>
{
    public void Configure(EntityTypeBuilder<CambioEstadoPedido> builder)
    {
        builder.ToTable("PedidoHistorialEstados");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.EstadoAnterior).HasMaxLength(20);
        builder.Property(c => c.EstadoNuevo).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Fecha).IsRequired();
        builder.HasIndex(c => new { c.PedidoId, c.Secuencia }).IsUnique();
    }
}
