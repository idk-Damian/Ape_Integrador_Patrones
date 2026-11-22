using LogiMarket.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Configurations;

public class EventoOutboxConfiguration : IEntityTypeConfiguration<EventoOutbox>
{
    public void Configure(EntityTypeBuilder<EventoOutbox> builder)
    {
        builder.ToTable("EventosOutbox");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TipoEvento).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).IsRequired();
        builder.Property(e => e.FechaCreacion).IsRequired();
        builder.Property(e => e.UltimoError).HasMaxLength(500);

        builder.HasIndex(e => e.IdEvento).IsUnique();
        builder.HasIndex(e => e.FechaProcesado);

        builder.Ignore(e => e.Procesado);
    }
}
