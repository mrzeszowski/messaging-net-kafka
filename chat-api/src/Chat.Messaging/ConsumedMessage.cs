namespace Chat.Messaging;

internal sealed class ConsumedMessage(
    long offset,
    int partition,
    string partitionKey,
    byte[] payload,
    IReadOnlyDictionary<string, string> headers,
    string topic,
    string groupId)
{
    public long Offset => offset;

    public long Partition => partition;

    public string PartitionKey => partitionKey;

    public byte[] Payload => payload;

    public IReadOnlyDictionary<string, string> Headers => headers;

    public string Topic => topic;

    public string GroupId => groupId;
}