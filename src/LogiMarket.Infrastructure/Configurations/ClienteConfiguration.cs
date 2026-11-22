using LogiMarket.Domain.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Categoria).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Activo).IsRequired();

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Nombre);
    }
}
