using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Messaging.Ef;

internal sealed class EventOutboxEfConfiguration : IEntityTypeConfiguration<EventOutbox>
{
    public void Configure(EntityTypeBuilder<EventOutbox> builder)
    {
        builder.ToTable("EventOutbox");

        builder.HasKey(x => x.LocalOffset);
        builder.Property(x => x.LocalOffset).ValueGeneratedOnAdd();
        builder.Property(x => x.Id);
        builder.Property(x => x.Payload);
        builder.Property(x => x.Timestamp);
        builder.Property(x => x.Topic);
        builder.Property(x => x.PartitionKey);
        builder.Property(x => x.Type);
    }
}