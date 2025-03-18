using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Chat.Messaging.Kafka;

internal class KafkaProducer : IProducer
{
    private readonly IProducer<byte[],byte[]> _producer;

    public KafkaProducer(IOptions<KafkaOptions> options)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            SecurityProtocol = options.Value.SecurityProtocol,
            SaslMechanism = options.Value.SaslMechanism,
            SaslUsername = options.Value.SaslUsername,
            SaslPassword = options.Value.SaslPassword,
            // Debug = "broker,topic"
        };
        _producer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
    }
    
    public Task ProduceAsync(ProducerMessage producerMessage, CancellationToken cancellationToken)
    {
        var message = new Message<byte[], byte[]>
        {
            Key = producerMessage.PartitionKey,
            Value = producerMessage.Payload,
            Headers =
            [
                new Header("message.id", Encoding.UTF8.GetBytes(producerMessage.Id)),
                new Header("message.type", Encoding.UTF8.GetBytes(producerMessage.Type))
            ],
        };
        
        return _producer.ProduceAsync(producerMessage.Topic, message, cancellationToken);
    }
}