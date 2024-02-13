using BotLife.Application.Arena;
using BotLife.Application.Engine.Clone;
using BotLife.Application.Shared;
using MediatR;

namespace BotLife.Application.Bot.MuBot;

public class MuBot : IBot
{
    private readonly IMediator _mediator;
    private readonly IRandomizer _randomizer;
    private readonly IArena _arena;
    private readonly IBotActParametersProvider _actParametersProvider;
    private int _cycle;
    private Direction _currentDirection = Direction.None;
    private int _stepsInCurrentDirection;
    private double _energy;
    private int _maxStepsSameDirection = 10;
    private Act _lastAction = Act.Empty;
    private const int CloneMinAge = 1;
    private const int CloneMaxAge = 10;
    private const int CloneMinEnergy = 10;

    public Guid Id { get; } = Guid.NewGuid();
    public BotType Type { get; } = BotType.MuBot;
    public double Energy => _energy;

    /// <summary>
    /// The age of the bot is calculated based on the number of cycles.
    /// For this bot age 1 = 1000 cycles => 1m 40sec
    /// (Assuming that the 1 cycle is 100ms)
    /// </summary>
    public int Age => _cycle / _actParametersProvider.GetAgeFactor();
    public Position Position { get; private set; } = Position.Empty;

    public MuBot(IMediator mediator, IRandomizer randomizer, IArena arena, IBotActParametersProvider actParametersProvider)
    {
        _mediator = mediator;
        _randomizer = randomizer;
        _arena = arena;
        _actParametersProvider = actParametersProvider;
        _energy = _actParametersProvider.GetEnergy();
    }

    public void SetPosition(Position position)
    {
        Position = position;
    }

    public void Next()
    {
        _cycle++;
        _energy -= CycleEnergy();

        var events = Scan();
        var act = React(events);
        Run(act);
        Clone();
    }

    public bool IsAlive()
    {
        return _energy > 0;
    }

    public void Rip()
    {
        _energy = 0;
    }

    public IEnumerable<Event> Scan()
    {
        if (!IsTimeToMove())
        {
            return new List<Event>();
        }

        return _arena.Scan(this, Position, _actParametersProvider.GetScanArea());
    }

    public Act React(IEnumerable<Event> events)
    {
        if (!IsTimeToMove())
        {
            return new Act(Event.Empty, ActType.None);
        }
        
        return GetBestAction(events);
    }

    private Act GetBestAction(IEnumerable<Event> events)
    {
        if (_lastAction.Type == ActType.Catch)
        {
            return _lastAction;
        }
        
        var @event = events.FirstOrDefault() ?? Event.Empty;
        _lastAction = @event.Type switch
        {
            EventType.FoundPsiBot => Energy >= 100
                ? new Act(Event.Empty, ActType.WalkAround)
                : new Act(@event, ActType.Catch),
            _ => new Act(Event.Empty, ActType.WalkAround)
        };

        return _lastAction;
    }

    public void Run(Act act)
    {
        if (!IsTimeToMove())
        {
            return;
        }

        if (act.Type == ActType.Catch)
        {
            var psiBot = act.Event.To;
            var psiPosition = psiBot.Position;
            if (Position == psiPosition)
            {
                _energy += EatEnergy(psiBot);
                psiBot.Rip();
                _lastAction = Act.Empty;
            }
            else
            {
                var nextPosition = _arena.MoveTo(this, psiBot);
                Position = nextPosition;
                _energy -= WalkEnergy();
            }
        }
        else if (act.Type == ActType.WalkAround)
        {
            var directions = GetAllDirections();
            if (_stepsInCurrentDirection > _maxStepsSameDirection || _currentDirection == Direction.None)
            {
                _randomizer.Shuffle(directions);
                _currentDirection = (Direction) directions[0];
                _stepsInCurrentDirection = 0;
            }

            var nextPosition = Move(_currentDirection);
            while (Position == nextPosition && directions.Length > 1)
            {
                directions = RemoveDirection(directions, (int) _currentDirection);
                _randomizer.Shuffle(directions);
                _currentDirection = (Direction) directions[0];
                nextPosition = Move(_currentDirection);
            }

            if (Position != nextPosition)
            {
                _stepsInCurrentDirection++;
                Position = nextPosition;

                _energy -= WalkEnergy();
            }
        }
    }

    public void Clone()
    {
        if (Age is > CloneMinAge and < CloneMaxAge && _energy > CloneMinEnergy)
        {
            // Avoid cloning all at the same time.
            var nextGeneration = _randomizer.Rnd(0, 100);
            
            // New generation once a year.
            if (_cycle % (_actParametersProvider.GetAgeFactor() + nextGeneration) != 0) return;

            _mediator.Send(new CloneCommand(new MuBot(_mediator, _randomizer, _arena, _actParametersProvider)));
        }
    }

    public bool IsTimeToMove()
    {
        return _cycle % _actParametersProvider.GetStepFrequency() == 0;
    }

    public double CycleEnergy()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.5, 4);
    }

    public double WalkEnergy()
    {
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1) * 0.5, 4);
    }
    
    public double EatEnergy(IBot bot)
    {
        return (WalkEnergy() + CycleEnergy()) * 300;
    }

    private Position Move(Direction direction)
    {
        return _arena.MoveTo(this, Position, direction);
    }

    private int[] GetAllDirections()
    {
        return
        [
            (int)Direction.Up, (int)Direction.Down, (int)Direction.Left, (int)Direction.Right
        ];
    }

    private int[] RemoveDirection(int[] directions, int direction)
    {
        return directions.Where(d => d != direction).ToArray();
    }
}