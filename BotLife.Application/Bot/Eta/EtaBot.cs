using BotLife.Application.Arena;
using BotLife.Application.Engine.Clone;
using BotLife.Application.Shared;
using MediatR;

namespace BotLife.Application.Bot.Eta;

public class EtaBot : IBot
{
    private readonly IMediator _mediator;
    private readonly IRandomizer _randomizer;
    private readonly IArena _arena;
    private readonly IBotActParametersProvider _actParametersProvider;
    private int _cycle;
    private Direction _currentDirection = Direction.None;
    private int _stepsInCurrentDirection;
    private double _energy;
    private int _speed;
    private int _maxStepsSameDirection = 10;
    private Act _lastAction = Act.Empty;
    private const int CloneMinAge = 1;
    private const int CloneMaxAge = 9;
    private const int CloneMinEnergy = 5;

    public Guid Id { get; } = Guid.NewGuid();
    public BotType Type { get; } = BotType.Eta;
    public double Energy => _energy;

    /// <summary>
    /// The age of the bot is calculated based on the number of cycles.
    /// For this bot age 1 = 1000 cycles => 1m 40sec
    /// (Assuming that the 1 cycle is 100ms)
    /// </summary>
    public int Age => _cycle / _actParametersProvider.GetAgeFactor();
    public Position Position { get; private set; } = Position.Empty;

    public EtaBot(IMediator mediator, IRandomizer randomizer, IArena arena, IBotActParametersProvider actParametersProvider)
    {
        _mediator = mediator;
        _randomizer = randomizer;
        _arena = arena;
        _actParametersProvider = actParametersProvider;
        _energy = _actParametersProvider.GetEnergy();
        _speed = _actParametersProvider.GetStepFrequency();
    }

    public void SetPosition(Position position)
    {
        Position = position;
    }

    public void Next()
    {
        _cycle++;
        _energy -= CycleEnergy();
        _speed = CalibrateSpeed();

        var events = Scan();
        _lastAction = React(events);
        Run(_lastAction);
        Clone();
    }

    public bool IsAlive()
    {
        return _energy > 0 && Age < _actParametersProvider.GetMaxAge();
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
        var @event = events.FirstOrDefault() ?? Event.Empty;
        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Energy >= _actParametersProvider.GetEnergy()
                ? new Act(Event.Empty, ActType.WalkAround)
                : new Act(@event, ActType.Catch),
            _ => new Act(Event.Empty, ActType.WalkAround)
        };

        // Keep catching the same bot
        if (_lastAction.Type == ActType.Catch && nextAction.Type == ActType.Catch)
        {
            return _lastAction;
        }

        // If bot escaped then stop running.
        if (_lastAction.Type == ActType.Catch && nextAction.Type != ActType.Catch ||
            _lastAction.Type != ActType.Catch)
        {
            _speed = _actParametersProvider.GetStepFrequency();
        }

        return nextAction;
    }

    public void Run(Act act)
    {
        if (!IsTimeToMove())
        {
            return;
        }

        switch (act.Type)
        {
            case ActType.Catch:
                Catch(act);
                break;
            case ActType.WalkAround:
                Walk();
                break;
        }
    }

    public void Clone()
    {
        if (Age is > CloneMinAge and < CloneMaxAge && _energy > CloneMinEnergy)
        {
            // Avoid cloning all at the same time.
            var nextGeneration = _randomizer.Rnd(0, 40);
            
            // New generation once a year.
            if (_cycle % (_actParametersProvider.GetAgeFactor() + nextGeneration) != 0) return;

            _mediator.Send(new CloneCommand(new EtaBot(_mediator, _randomizer, _arena, _actParametersProvider)));
        }
    }

    public bool IsTimeToMove()
    {
        return _cycle % _speed == 0;
    }

    public double CycleEnergy()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.5, 4);
    }

    public double WalkEnergy()
    {
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1) * 0.5, 4);
    }

    public double RunEnergy()
    {
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1) * 0.8, 4);
    }

    public double EatEnergy(IBot bot)
    {
        return (WalkEnergy() + CycleEnergy()) * _actParametersProvider.GetAgeFactor() * 0.8;
    }

    private void Walk()
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

    private void Catch(Act act)
    {
        _currentDirection = Direction.None;
        var muBot = act.Event.To;
        var muPosition = muBot.Position;
        if (Position == muPosition)
        {
            _energy += EatEnergy(muBot);
            muBot.Rip();
            _lastAction = Act.Empty;
            _speed = _actParametersProvider.GetStepFrequency();
        }
        else
        {
            _speed = TryToRun();
            var nextPosition = _arena.MoveTo(this, muBot);
            Position = nextPosition;
            _energy -= CanRun() ? RunEnergy() : WalkEnergy();
        }
    }

    private int CalibrateSpeed()
    {
        if (!CanRun())
        {
            return _actParametersProvider.GetStepFrequency();
        }

        return _speed;
    }

    private bool CanRun()
    {
        return Energy > 50;
    }

    private int TryToRun()
    {
        return CanRun()
            ? _actParametersProvider.GetStepFrequency() / 2
            : _actParametersProvider.GetStepFrequency();
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