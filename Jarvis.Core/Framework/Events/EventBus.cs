namespace Jarvis.Core.Framework.Events;

/// <summary>
/// Provides an in-memory event bus for framework events.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Guid, EventSubscription> subscriptions = [];
    private readonly object gate = new();

    /// <inheritdoc />
    public Guid Subscribe(string eventName, Func<FrameworkEvent, CancellationToken, Task> handler)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name is required.", nameof(eventName));
        }

        ArgumentNullException.ThrowIfNull(handler);

        var subscription = new EventSubscription(Guid.NewGuid(), eventName, handler);
        lock (gate)
        {
            subscriptions[subscription.Id] = subscription;
        }

        return subscription.Id;
    }

    /// <inheritdoc />
    public bool Unsubscribe(Guid subscriptionId)
    {
        lock (gate)
        {
            return subscriptions.Remove(subscriptionId);
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(FrameworkEvent frameworkEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frameworkEvent);

        EventSubscription[] matchingSubscriptions;
        lock (gate)
        {
            matchingSubscriptions = subscriptions.Values
                .Where(subscription => string.Equals(subscription.EventName, frameworkEvent.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        foreach (var subscription in matchingSubscriptions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await subscription.Handler(frameworkEvent, cancellationToken);
        }
    }

    private sealed record EventSubscription(
        Guid Id,
        string EventName,
        Func<FrameworkEvent, CancellationToken, Task> Handler);
}
