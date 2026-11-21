using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClienteId).IsRequired();
        builder.Property(p => p.CategoriaCliente).IsRequired();

        builder.Property(p => p.EstadoNombre)
            .HasColumnName("Estado")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NombreDescuento).HasMaxLength(20);
        builder.Property(p => p.TarifaEnvio).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Total).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TipoEntrega).IsRequired().HasMaxLength(20);

        builder.Property(p => p.FechaCreacion).IsRequired();

        // FK con Cliente
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Propiedades computadas – no se persisten
        builder.Ignore(p => p.PuedeEditar);
        builder.Ignore(p => p.PuedeDeshacer);
        builder.Ignore(p => p.PuedeRehacer);
        builder.Ignore(p => p.TotalUnidadesFisicas);

        // Navegación líneas con backing field
        builder.HasMany(p => p.Lineas)
            .WithOne()
            .HasForeignKey(l => l.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Lineas)
            .HasField("_lineas")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
