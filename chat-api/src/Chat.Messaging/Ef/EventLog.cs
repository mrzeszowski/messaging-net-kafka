namespace Chat.Messaging.Ef;

internal sealed class EventLog(
    long localOffset,
    Guid id,
    string type,
    byte[] payload,
    long timestamp,
    string topic,
    byte[] partitionKey)
{
    public long LocalOffset { get; } = localOffset;

    public Guid Id { get; } = id;

    public string Type { get; } = type;

    public byte[] Payload { get; } = payload;

    public long Timestamp { get; } = timestamp;

    public string Topic { get; } = topic;

    public byte[] PartitionKey { get; } = partitionKey;
}