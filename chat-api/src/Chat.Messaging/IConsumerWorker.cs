namespace Chat.Messaging;

internal interface IConsumerWorker
{
    Task ConsumeAsync(CancellationToken cancellationToken);
}