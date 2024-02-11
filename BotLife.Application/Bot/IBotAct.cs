namespace BotLife.Application.Bot;

public interface IBotAct
{
    IEnumerable<Event> Scan();
    Act React(IEnumerable<Event> e);
    void Run(Act act);
}