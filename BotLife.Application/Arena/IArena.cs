using BotLife.Contracts;

namespace BotLife.Application.Arena;

public interface IArena
{
    void AddBotAtRandom(IBot bot);
    void AddBotAtPosition(IBot bot, Position position);
    IEnumerable<Event> Scan(IBot bot, Position position, int range);
    Position MoveTo(IBot bot, Position from, IEnumerable<Direction> directions);
    Position MoveTo(IBot bot, Position from, Direction where);
    Position MoveTo(IBot bot, IBot botToFollow);
    void RemoveBot(IBot bot);
    void BuildArena(int maxWidth, int maxHeight);
    bool IsInitialized { get; }
    IEnumerable<IBot> GetBotsAt(Position position);
}