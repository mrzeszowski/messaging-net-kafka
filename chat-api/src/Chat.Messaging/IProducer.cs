namespace Chat.Messaging;

internal interface IProducer
{
    Task ProduceAsync(ProducerMessage producerMessage, CancellationToken cancellationToken);
}