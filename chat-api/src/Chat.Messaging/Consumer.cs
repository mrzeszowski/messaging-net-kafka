namespace Chat.Messaging;

internal class Consumer
{
    public Subscription Subscription { get; }

    public Task WorkerTask { get; }

    public Consumer(Subscription subscription, IConsumerWorker worker, CancellationToken cancellationToken)
    {
        var consumerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Subscription = subscription;
        WorkerTask = worker.ConsumeAsync(consumerCancellationTokenSource.Token);
    }
}