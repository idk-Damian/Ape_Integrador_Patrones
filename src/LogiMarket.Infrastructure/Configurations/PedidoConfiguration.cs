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

        builder.Property(p => p.EstadoNombre).HasColumnName("Estado").IsRequired().HasMaxLength(20);
        builder.Property(p => p.CategoriaCliente).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TipoEntrega).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.NombreDescuento).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TarifaEnvio).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Total).HasColumnType("decimal(18,2)");
        builder.Property(p => p.FechaCreacion).IsRequired();
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.HasIndex(p => p.EstadoNombre);
        builder.HasIndex(p => p.ClienteId);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Lineas)
            .WithOne()
            .HasForeignKey(l => l.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Lineas).HasField("_lineas").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.CambiosEstado)
            .WithOne()
            .HasForeignKey(c => c.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.CambiosEstado).HasField("_cambiosEstado").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.PuedeEditar);
        builder.Ignore(p => p.PuedeDeshacer);
        builder.Ignore(p => p.PuedeRehacer);
        builder.Ignore(p => p.TotalUnidadesFisicas);
        builder.Ignore(p => p.ImporteProductos);
    }
}
