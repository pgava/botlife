using BotLife.Application.Bot;
using BotLife.Application.Shared.Exceptions;

namespace BotLife.Application.Arena;

internal class BotSquare
{
    public List<IBot> Bots { get; }

    public static BotSquare Create(IBot bot)
    {
        return new BotSquare(bot);
    }

    public static BotSquare Empty()
    {
        return new BotSquare();
    }

    private BotSquare()
    {
        Bots = new List<IBot>();
    }

    private BotSquare(IBot bot)
    {
        Bots = new List<IBot> {bot};
    }

    private void Add(IBot bot)
    {
        Bots.Add(bot);
    }

    public void Remove(IBot bot)
    {
        if (Bots.Count > 0)
        {
            Bots.Remove(bot);
        }
    }

    public static BotSquare operator +(BotSquare square, IBot bot)
    {
        square.Add(bot);
        return square;
    }

    public static BotSquare operator -(BotSquare square, IBot bot)
    {
        square.Remove(bot);
        return square;
    }

    public bool IsEmpty()
    {
        return Bots.Count == 0;
    }
}

public class BotArena : IArena
{
    private readonly ICollisionManager _collisionManager;
    private BotSquare[,] _map;
    private List<Position> _freePositions;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    public BotArena(ICollisionManager collisionManager)
    {
        _collisionManager = collisionManager;
    }

    public void BuildArena(int maxWidth, int maxHeight)
    {
        _map = new BotSquare[maxWidth, maxHeight];
        _freePositions = new List<Position>();
        Init();
    }

    // Add a bot at random position on the map.
    public void AddBotAtRandom(IBot bot)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");
        ArenaFullException.ThrowIfFull(_freePositions, "No empty position found.");

        CheckSpace();

        // Find a random position on the map.
        var random = new Random();
        var freeSlot = random.Next(0, _freePositions.Count - 1);
        var position = _freePositions[freeSlot];
        bot.SetPosition(position);
        _map[position.X, position.Y] = BotSquare.Create(bot);
        _freePositions.RemoveAt(freeSlot);

        CheckSpace();
    }

    // Add a bot at the given position on the map.
    public void AddBotAtPosition(IBot bot, Position position)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");
        ArenaFullException.ThrowIfFull(_freePositions, "No empty position found.");

        CheckSpace();

        bot.SetPosition(position);
        _map[position.X, position.Y] += bot;
        _freePositions.Remove(position);

        CheckSpace();
    }

    // Remove a bot from the map.
    public Position MoveTo(IBot bot, IBot botToFollow)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");

        var from = bot.Position;
        var to = botToFollow.Position;
        var directions = new List<Direction>();
        if (from.X < to.X)
        {
            directions.Add(Direction.Right);
        }
        else if (from.X > to.X)
        {
            directions.Add(Direction.Left);
        }

        if (from.Y < to.Y)
        {
            directions.Add(Direction.Down);
        }
        else if (from.Y > to.Y)
        {
            directions.Add(Direction.Up);
        }

        return MoveTo(bot, from, directions);
    }

    public void RemoveBot(IBot bot)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");

        CheckSpace();

        _map[bot.Position.X, bot.Position.Y] -= bot;

        if (_map[bot.Position.X, bot.Position.Y].IsEmpty())
        {
            _freePositions.Add(bot.Position);
        }

        CheckSpace();
    }

    // Scan an area around the given position and return a list of events based on what bots are found.
    public IEnumerable<Event> Scan(IBot bot, Position position, int range)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");

        var events = new List<Event>();
        for (var x = position.X - range; x <= position.X + range; x++)
        {
            for (var y = position.Y - range; y <= position.Y + range; y++)
            {
                if (x >= 0 && x < _map.GetLength(0) && y >= 0 && y < _map.GetLength(1))
                {
                    foreach (var b in _map[x, y].Bots)
                    {
                        if (bot == b)
                        {
                            continue;
                        }

                        events.Add(b.Type switch
                        {
                            BotType.Mu => Event.Trigger(EventType.FoundMu, bot, b),
                            BotType.Psi => new Event(EventType.FoundPsi, bot, b),
                            BotType.Eta => new Event(EventType.FoundEta, bot, b),
                            _ => new Event(EventType.None, bot, b)
                        });
                    }
                }
            }
        }

        return events.Where(e => e.Type != EventType.None);
    }

    // Return the first position where the given bot can be placed.
    public Position MoveTo(IBot bot, Position from, IEnumerable<Direction> directions)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");

        var to = from;
        foreach (var direction in directions)
        {
            to = CanMoveTo(bot, to, direction);
            if (to != from)
            {
                break;
            }
        }

        AssignPosition(bot, from, to);

        return to;
    }

    // Return the position after moving from the given position in the given direction
    // for the given number of steps.
    public Position MoveTo(IBot bot, Position from, Direction where)
    {
        ArenaNotInitializedException.ThrowIfNotInitialized(this, "Arena not initialized.");

        var to = CanMoveTo(bot, from, where);

        AssignPosition(bot, from, to);

        return to;
    }

    public IEnumerable<IBot> GetBotsAt(Position position)
    {
        return _map[position.X, position.Y].Bots;
    }

    private void AssignPosition(IBot bot, Position from, Position to)
    {
        CheckSpace();

        if (to != from)
        {
            _map[to.X, to.Y] += bot;
            _map[from.X, from.Y] -= bot;

            _freePositions.Remove(to);
            if (_map[from.X, from.Y].IsEmpty())
            {
                _freePositions.Add(from);
            }
        }

        CheckSpace();
    }

    private Position CanMoveTo(IBot bot, Position from, Direction where)
    {
        CheckSpace();

        var to = where switch
        {
            Direction.Right => new Position(
                CheckXPath(bot, from.X, int.Min(from.X + 1, _map.GetLength(0) - 1), from.Y), from.Y),
            Direction.Left => new Position(
                CheckXPath(bot, from.X, int.Max(from.X - 1, 0), from.Y), from.Y),
            Direction.Up => new Position(
                from.X, CheckYPath(bot, from.Y, int.Max(from.Y - 1, 0), from.X)),
            Direction.Down => new Position(
                from.X, CheckYPath(bot, from.Y, int.Min(from.Y + 1, _map.GetLength(1) - 1), from.X)),
            _ => from
        };

        CheckSpace();

        return to;
    }

    // Initialize the map with empty positions.
    private void Init()
    {
        for (var x = 0; x < _map.GetLength(0); x++)
        {
            for (var y = 0; y < _map.GetLength(1); y++)
            {
                _map[x, y] = BotSquare.Empty();
                _freePositions.Add(Position.At(x, y));
            }
        }
        _isInitialized = true;

        CheckSpace();
    }

    private void CheckSpace()
    {
#if DEBUG
        // Loop through the map and count how many empty positions are left.
        var available = 0;
        foreach (var botSquare in _map)
        {
            if (botSquare.IsEmpty())
            {
                available++;
            }
        }

        ArenaSpaceNotMatchException.ThrowIfNotMatch(
            _freePositions.Count,
            available,
            "Not the same: available = {available}, freePositions = {_freePositions.Count}");
#endif
    }

    // Check the x path from the given position and the final position is empty, because sometime bots cannot
    // move through other bots.
    private int CheckXPath(IBot bot, int from, int to, int y)
    {
        return _collisionManager.CanCollide(bot, _map[to, y].Bots) ? to : from;
    }

    // Check the y path from the given position and the final position is empty, because sometime bots cannot
    // move through other bots.
    private int CheckYPath(IBot bot, int from, int to, int x)
    {
        return _collisionManager.CanCollide(bot, _map[x, to].Bots) ? to : from;
    }
}