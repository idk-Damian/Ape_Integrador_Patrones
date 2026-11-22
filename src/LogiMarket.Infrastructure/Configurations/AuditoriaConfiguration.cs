using LogiMarket.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Categoria).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(60);
        builder.Property(a => a.Referencia).IsRequired().HasMaxLength(80);
        builder.Property(a => a.Usuario).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Detalle).HasMaxLength(1000);
        builder.Property(a => a.Correlacion).HasMaxLength(40);
        builder.Property(a => a.Fecha).IsRequired();

        builder.HasIndex(a => a.Categoria);
        builder.HasIndex(a => a.Fecha);
    }
}
