namespace Chat.Messaging;

internal class ProducerMessage(string id, string type, byte[] partitionKey, byte[] payload, string topic)
{
    public string Id { get; } = id;
    public string Type { get; } = type;
    public byte[] PartitionKey { get; } = partitionKey;
    public byte[] Payload { get; } = payload;
    public string Topic { get; } = topic;
}