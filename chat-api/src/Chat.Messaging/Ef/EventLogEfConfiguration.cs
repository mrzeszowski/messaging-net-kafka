using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Messaging.Ef;

internal sealed class EventLogEfConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("EventLog");

        builder.HasKey(x => x.LocalOffset);
        builder.Property(x => x.LocalOffset).ValueGeneratedNever();
        builder.Property(x => x.Id);
        builder.Property(x => x.Payload);
        builder.Property(x => x.Timestamp);
        builder.Property(x => x.Topic);
        builder.Property(x => x.PartitionKey);
        builder.Property(x => x.Type);
        
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => new { x.Type, x.PartitionKey });
        builder.HasIndex(x => x.Topic);
        builder.Property(x => x.PartitionKey);
    }
}