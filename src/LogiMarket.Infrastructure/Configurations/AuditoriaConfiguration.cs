using LogiMarket.Domain.Outbox;
using LogiMarket.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidad).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntidadId).IsRequired();
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Fecha).IsRequired();
        builder.Property(a => a.Detalle).HasMaxLength(1000);

        builder.HasIndex(a => new { a.Entidad, a.EntidadId });

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
