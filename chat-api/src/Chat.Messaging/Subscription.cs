namespace Chat.Messaging;

internal class Subscription
{
    public Guid Id = Guid.NewGuid();
    public IReadOnlyCollection<string> Topics { get; init; } = [];
    public int TasksCount { get; init; } = 3;
}