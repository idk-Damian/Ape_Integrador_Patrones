using LogiMarket.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(64);
        builder.Property(u => u.Salt).IsRequired().HasMaxLength(32);
        builder.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.Activo).IsRequired();
    }
}
