namespace BotLife.Application.Bot;

/// <summary>
/// Event representation.
/// </summary>
/// <param name="Type">Type of event.</param>
public record Event(EventType Type, IBot From, IBot To)
{
    public static Event Empty => new(EventType.None, EmptyBot.Instance, EmptyBot.Instance);

    public static Event Trigger(EventType type, IBot from, IBot to) => new(type, from, to);
}

public static class EventsExtensions
{
    public static Act ToAction(this IEnumerable<Event> events, Func<IEnumerable<Event>, Act> react)
    {
        return react(events);
    }
}