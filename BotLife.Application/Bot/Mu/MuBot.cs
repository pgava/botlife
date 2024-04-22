using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.DataAccess.Models;
using BotLife.Application.Engine.Clone;
using BotLife.Application.Shared;
using MediatR;
using Serilog;

namespace BotLife.Application.Bot.Mu;

public class MuBot : IBot
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IRandomizer _randomizer;
    private readonly IArena _arena;
    private readonly IBotActParametersProvider _actParametersProvider;
    private int _cycle;
    private Direction _currentDirection = Direction.None;
    private int _stepsInCurrentDirection;
    private double _energy;
    private int _speed;
    private readonly int _maxStepsSameDirection = 10;
    private readonly int _nextGeneration;
    private Act _lastAction = Act.Empty;
    private const int CloneMinAge = 1;
    private const int CloneMaxAge = 4;
    private const int CloneMinEnergy = 5;

    public Guid Id { get; } = Guid.NewGuid();
    public BotType Type { get; } = BotType.Mu;
    public double Energy => _energy;

    /// <summary>
    /// The age of the bot is calculated based on the number of cycles.
    /// For this bot age 1 = 1000 cycles => 1m 40sec
    /// (Assuming that the 1 cycle is 100ms)
    /// </summary>
    public int Age => _cycle / _actParametersProvider.GetYearCycles();
    public Position Position { get; private set; } = Position.Empty;

    public MuBot(ILogger logger, IMediator mediator, IRandomizer randomizer, IArena arena,
        IBotActParametersProvider actParametersProvider)
    {
        _logger = logger;
        _mediator = mediator;
        _randomizer = randomizer;
        _arena = arena;
        _actParametersProvider = actParametersProvider;
        _energy = _actParametersProvider.GetEnergy();
        _speed = _actParametersProvider.GetStepFrequency();
        // Avoid cloning all at the same time.
        _nextGeneration = _randomizer.Rnd(0, 50);
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

        Scan()
            .ToAction(ChooseAction)
            .Do(Run);

        Clone();
    }
    
    public bool IsAlive()
    {
        var isAlive = _energy > 0 && Age < _actParametersProvider.GetMaxAge();
        if (!isAlive)
        {
            _logger.Debug("Bot {@Bot} is dead", BotIdentity.Create(this));
        }

        return isAlive;
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

    public Act ChooseAction(IEnumerable<Event> events)
    {
        if (!IsTimeToMove())
        {
            return _lastAction;
        }
        
        return GetBestAction(events);
    }
    
    public void Run(Act act)
    {
        if (_lastAction != act)
        {
            _mediator.Send(new LogEventCommand(act, _energy, EventStatus.Pending));            
        }
        
        _lastAction = act;

        if (!IsTimeToMove())
        {
            return;
        }

        _logger.Debug("Bot {@Bot} is running act {ActType}", BotIdentity.Create(this), act.Type);

        switch (act.Type)
        {
            case ActType.Escape:
                Escape(act);
                break;
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
        if (!CanClone()) return;
        
        // New generation.
        if (_cycle % (_actParametersProvider.GetYearCycles() / 2) != _nextGeneration) return;
        
        _logger.Debug("Bot {@Bot} is cloning", BotIdentity.Create(this));

        _mediator.Send(new CloneCommand(new MuBot(_logger, _mediator, _randomizer, _arena, _actParametersProvider)));
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
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1), 4);
    }

    public double EatEnergy(IBot bot)
    {
        return (WalkEnergy() + CycleEnergy()) * _actParametersProvider.GetYearCycles() * 0.5;
    }

    private Act GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is a Eta in the area then try to escape.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundEta) ??
                     (eventList.FirstOrDefault() ?? Event.Empty);

        var nextAction = @event.Type switch
        {
            EventType.FoundPsi => Energy >= _actParametersProvider.GetEnergy()
                ? Act.Trigger(Event.Empty, ActType.WalkAround)
                : Act.Trigger(@event, ActType.Catch),
            EventType.FoundEta => Act.Trigger(@event, ActType.Escape),
            _ => Act.Trigger(Event.Empty, ActType.WalkAround)
        };

        // Keep catching the same bot
        if (_lastAction.Type == ActType.Catch && nextAction.Type == ActType.Catch)
        {
            return _lastAction;
        }

        // If bot escaped then stop running.
        if (_lastAction.Type == ActType.Escape && nextAction.Type != ActType.Escape ||
            _lastAction.Type != ActType.Escape)
        {
            _speed = _actParametersProvider.GetStepFrequency();
        }

        return nextAction;
    }

    private void Escape(Act act)
    {
        _currentDirection = Direction.None;
        var eta = act.Event.From;
        var etaPosition = eta.Position;
        var directions = GetOppositeDirection(etaPosition);
        _speed = TryToRun();

        var nextPosition = _arena.MoveTo(this, etaPosition, directions);
        Position = nextPosition;
        _energy -= CanRun() ? RunEnergy() : WalkEnergy();
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
    
    private IEnumerable<Direction> GetOppositeDirection(Position other)
    {
        var directions = new List<Direction>();

        var x = Position.X - other.X;
        var y = Position.Y - other.Y;
        if (Math.Abs(x) > Math.Abs(y))
        {
            directions.Add(x > 0 ? Direction.Right : Direction.Left);
            directions.Add(y > 0 ? Direction.Down : Direction.Up);
        }
        else
        {
            directions.Add(y > 0 ? Direction.Down : Direction.Up);
            directions.Add(x > 0 ? Direction.Right : Direction.Left);
        }

        return directions;
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
        return Energy > 30;
    }
    
    private bool CanClone()
    {
        return Age is > CloneMinAge and < CloneMaxAge && _energy > CloneMinEnergy;
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