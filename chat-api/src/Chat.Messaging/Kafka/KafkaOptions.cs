using Confluent.Kafka;

namespace Chat.Messaging.Kafka;

internal sealed class KafkaOptions
{
    private readonly string? _saslPassword;
    private readonly string? _saslUsername;
    public string BootstrapServers { get; init; } = "";

    public string? SaslUsername
    {
        get => _saslUsername == "" ? null : _saslUsername;
        init => _saslUsername = value;
    }

    public string? SaslPassword
    {
        get => _saslPassword == "" ? null : _saslPassword;
        init => _saslPassword = value;
    }

    public SaslMechanism? SaslMechanism { get; init; }
    public SecurityProtocol? SecurityProtocol { get; init; }
}