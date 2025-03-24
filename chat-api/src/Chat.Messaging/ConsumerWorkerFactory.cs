using Chat.Messaging.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Messaging;

internal class ConsumerWorkerFactory(ILoggerFactory loggerFactory, MessageProcessor processor, IOptions<KafkaOptions> options)
{
    public KafkaConsumerWorker Create(string consumerGroupId, Subscription subscription)
    {
        return new KafkaConsumerWorker(consumerGroupId: consumerGroupId,
            subscription: subscription,
            logger: loggerFactory.CreateLogger<KafkaConsumerWorker>(),
            processor: processor,
            options: options);
    }
}