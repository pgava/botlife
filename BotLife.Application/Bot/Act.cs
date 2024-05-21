namespace BotLife.Application.Bot;

public record Act(Event Event, ActType Type)
{
    public static Act Empty(IBot from, IBot to) => new(Event.Empty(from, to), ActType.None);
    public static Act Trigger(Event e, ActType type) => new(e, type);
}

public static class ActExtensions
{
    public static void Do(this Act act, Action<Act> react)
    {
        react(act);
    }
}