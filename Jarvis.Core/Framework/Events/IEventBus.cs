namespace Jarvis.Core.Framework.Events;

/// <summary>
/// Defines an in-process event bus for framework events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a handler to an event name.
    /// </summary>
    /// <param name="eventName">The event name to subscribe to.</param>
    /// <param name="handler">The event handler.</param>
    /// <returns>The subscription identifier.</returns>
    Guid Subscribe(string eventName, Func<FrameworkEvent, CancellationToken, Task> handler);

    /// <summary>
    /// Removes a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <returns><c>true</c> when the subscription was removed.</returns>
    bool Unsubscribe(Guid subscriptionId);

    /// <summary>
    /// Publishes an event to matching subscribers.
    /// </summary>
    /// <param name="frameworkEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token that cancels publication.</param>
    /// <returns>A task that completes when subscribers have been invoked.</returns>
    Task PublishAsync(FrameworkEvent frameworkEvent, CancellationToken cancellationToken = default);
}
