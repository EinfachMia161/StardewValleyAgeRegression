using AgeRegression.Utilities;

namespace AgeRegression.Events;

/// <summary>
/// A lightweight, typed publish/subscribe event bus for internal mod
/// communication. Systems subscribe to specific event types without
/// needing direct references to the systems that publish them.
///
/// <para>
/// Design note: This is intentionally simple. It is not thread-safe
/// because Stardew Valley is single-threaded. If multiplayer support
/// is added later, synchronization will need to be introduced here.
/// </para>
/// </summary>
public sealed class ModEventBus
{
    private readonly LogHelper _log;
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public ModEventBus(LogHelper log)
    {
        _log = log;
    }

    /// <summary>
    /// Subscribes a handler to events of type
    /// <typeparamref name="TEvent"/>.
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var type = typeof(TEvent);
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _handlers[type] = list;
        }

        list.Add(handler);
    }

    /// <summary>
    /// Unsubscribes a previously registered handler.
    /// </summary>
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var list))
            list.Remove(handler);
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// Exceptions in individual handlers are caught and logged so one
    /// failing subscriber does not prevent others from receiving the
    /// event.
    /// </summary>
    public void Publish<TEvent>(TEvent eventArgs) where TEvent : class
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var list)
            || list.Count == 0)
            return;

        // Snapshot the list to allow handlers to unsubscribe during
        // iteration without causing a collection-modified exception.
        foreach (var handler in list.ToArray())
        {
            try
            {
                ((Action<TEvent>)handler)(eventArgs);
            }
            catch (Exception ex)
            {
                _log.Exception(
                    $"Event handler for {typeof(TEvent).Name} threw an exception",
                    ex);
            }
        }
    }
}
