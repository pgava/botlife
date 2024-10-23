namespace BotLife.Contracts;

public record Activity(Event Event, ActivityType Type)
{
    public static Activity Empty(IBot from, IBot to) => new(Event.Empty(from, to), ActivityType.None);
    public static Activity Trigger(Event e, ActivityType type) => new(e, type);
}

public static class ActExtensions
{
    public static void Do(this Activity activity, Action<Activity> react)
    {
        react(activity);
    }
}