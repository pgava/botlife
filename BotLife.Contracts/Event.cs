namespace BotLife.Contracts;

/// <summary>
/// Event representation.
/// </summary>
/// <param name="Type">Type of event.</param>
public record Event(EventType Type, IBot From, IBot To)
{
    public static Event Empty(IBot from, IBot to) => new(EventType.None, from, to);
    public static Event Trigger(EventType type, IBot from, IBot to) => new(type, from, to);
}

public static class EventsExtensions
{
    public static Activity ToAction(this IEnumerable<Event> events, Func<IEnumerable<Event>, Activity> react)
    {
        return react(events);
    }
}