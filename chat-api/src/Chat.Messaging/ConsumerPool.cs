using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace Chat.Messaging;

internal class ConsumerPool(ConsumerWorkerFactory workerFactory, ILogger<ConsumerPool> logger)
{
    private readonly List<Consumer> _consumers = [];

    private CancellationToken? _cancellationToken;

    public Task AdjustConsumers(string consumerGroupId, IReadOnlyCollection<Subscription> subscriptions, CancellationToken cancellationToken)
    {
        _cancellationToken ??= cancellationToken;

        CleanupConsumers();
        CreateConsumers(consumerGroupId, subscriptions, cancellationToken);
        
        return Task.CompletedTask;
    }
    
    private void CreateConsumers(string consumerGroupId, IReadOnlyCollection<Subscription> subscriptions, CancellationToken cancellationToken)
    {
        foreach (var subscription in subscriptions)
        {
            var currentConsumersCount = _consumers.Count(x => x.Subscription.Id == subscription.Id);
            var missingConsumersCount = subscription.TasksCount - currentConsumersCount;
            
            _consumers.AddRange(Enumerable.Range(0, missingConsumersCount)
                .Select(x => new Consumer(subscription: subscription,
                    worker: workerFactory.Create(consumerGroupId, subscription),
                    cancellationToken: cancellationToken)));
        }
    }

    private void CleanupConsumers()
    {
        var completedConsumers = _consumers.Where(x => x.WorkerTask.IsCompleted && !IsInCompletion(x)).ToArray();
        foreach (var consumer in completedConsumers)
        {
            if (consumer.WorkerTask.IsFaulted)
            {
                logger.LogError(consumer.WorkerTask.Exception, consumer.WorkerTask.Status.ToString());
                var outOfMemoryException = consumer.WorkerTask.Exception!.InnerExceptions.FirstOrDefault(e => e is OutOfMemoryException);
                if (outOfMemoryException is not null)
                {
                    ExceptionDispatchInfo.Capture(outOfMemoryException).Throw();
                }
            }

            _consumers.Remove(consumer);
        }

        return;

        bool IsInCompletion(Consumer completedJob)
            => completedJob.WorkerTask.Status == TaskStatus.RanToCompletion && _cancellationToken!.Value.IsCancellationRequested;
    }
}