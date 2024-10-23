namespace BotLife.Contracts;

public interface IBotAction
{
    IEnumerable<Event> Scan();
    Activity React(IEnumerable<Event> e);
    void Run(Activity activity);
}