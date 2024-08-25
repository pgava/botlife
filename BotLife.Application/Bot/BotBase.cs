using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.Bot.Mu;
using BotLife.Application.DataAccess.Models;
using BotLife.Application.Engine.Clone;
using BotLife.Application.Shared;
using MediatR;
using Serilog;

namespace BotLife.Application.Bot;

public abstract class BotBase : IBot
{
    private readonly ILogger _logger;
    protected readonly IMediator Mediator;
    private readonly IRandomizer _randomizer;
    private readonly IArena _arena;
    protected readonly IBotActParametersProvider ActParametersProvider;
    private int _cycle;
    private Direction _currentDirection = Direction.None;
    private int _stepsInCurrentDirection;
    protected double CurrentEnergy;
    protected int Speed;
    private readonly int _maxStepsSameDirection = 10;
    private readonly int _nextGeneration;
    protected Act LastAction;
    private const int CloneMinAge = 1;
    private const int CloneMaxAge = 4;
    private const int CloneMinEnergy = 5;
    
    public Guid Id { get; } = Guid.NewGuid();
    public abstract BotType Type { get; }
    public double Energy => CurrentEnergy;
    public int Age => _cycle / ActParametersProvider.GetYearCycles();
    public Position Position { get; private set; } = Position.Empty;
    
    protected BotBase(ILogger logger, IMediator mediator, IRandomizer randomizer, IArena arena,
        IBotActParametersProvider parametersProvider)
    {
        _logger = logger;
        Mediator = mediator;
        _randomizer = randomizer;
        _arena = arena;
        ActParametersProvider = parametersProvider;
        CurrentEnergy = ActParametersProvider.GetEnergy();
        Speed = ActParametersProvider.GetStepFrequency();
        _nextGeneration = _randomizer.Rnd(0, 50);
        LastAction = Act.Empty(this, EmptyBot.Instance);
    }
    
    public void SetPosition(Position position)
    {
        Position = position;
    }

    public void Next()
    {
        _cycle++;
        CurrentEnergy -= CycleEnergy();
        Speed = CalibrateSpeed();

        Scan()
            .ToAction(ChooseAction)
            .Do(Run);

        Clone();
    }
    
    public bool IsAlive()
    {
        var isAlive = CurrentEnergy > 0 && Age < ActParametersProvider.GetMaxAge();
        if (!isAlive)
        {
            _logger.Debug("Bot {@Bot} is dead", BotIdentity.Create(this));
        }

        return isAlive;
    }

    public void Rip()
    {
        CurrentEnergy = 0;
        
        // Log the event.
        Mediator.Send(new LogEventCommand(Act.Empty(this, EmptyBot.Instance), CurrentEnergy, EventStatus.Completed));
    }

    public IEnumerable<Event> Scan()
    {
        if (!IsTimeToMove())
        {
            return new List<Event>();
        }

        return _arena.Scan(this, Position, ActParametersProvider.GetScanArea());
    }

    public Act ChooseAction(IEnumerable<Event> events)
    {
        if (!IsTimeToMove())
        {
            return LastAction;
        }
        
        return GetBestAction(events);
    }

    private void Run(Act act)
    {
        LastAction = act;

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

        if (CurrentEnergy <= 0)
        {
            Rip();
        }
    }

    private void Clone()
    {
        if (!CanClone()) return;
        
        // New generation.
        if (_cycle % (ActParametersProvider.GetYearCycles() / 2) != _nextGeneration) return;
        
        _logger.Debug("Bot {@Bot} is cloning", BotIdentity.Create(this));

        Mediator.Send(new CloneCommand(new MuBot(_logger, Mediator, _randomizer, _arena, ActParametersProvider)));
    }
    
    public bool IsTimeToMove()
    {
        return _cycle % Speed == 0;
    }

    public double CycleEnergy()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.5, 4);
    }

    public double WalkEnergy()
    {
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1) * 0.5, 4);
    }

    private double RunEnergy()
    {
        return Math.Round(Math.Log10(Math.Max(Age, 1) + 1), 4);
    }

    private double EatEnergy()
    {
        return (WalkEnergy() + CycleEnergy()) * ActParametersProvider.GetYearCycles() * 0.5;
    }

    protected abstract Act GetBestAction(IEnumerable<Event> events);
    

    private void Escape(Act act)
    {
        _currentDirection = Direction.None;
        var hunter = act.Event.From;
        var hunterPosition = hunter.Position;
        var directions = GetOppositeDirection(hunterPosition);
        Speed = TryToRun();

        var nextPosition = _arena.MoveTo(this, hunterPosition, directions);
        Position = nextPosition;
        CurrentEnergy -= CanRun() ? RunEnergy() : WalkEnergy();
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

            CurrentEnergy -= WalkEnergy();
        }
    }

    private void Catch(Act act)
    {
        _currentDirection = Direction.None;
        var prey = act.Event.To;
        var preyPosition = prey.Position;
        if (Position == preyPosition)
        {
            CurrentEnergy += EatEnergy();
            prey.Rip();
            
            // Log the event.
            Mediator.Send(new LogEventCommand(Act.Empty(this, EmptyBot.Instance), CurrentEnergy, EventStatus.Completed));

            LastAction = Act.Empty(this, EmptyBot.Instance);
            Speed = ActParametersProvider.GetStepFrequency();
        }
        else
        {
            Speed = TryToRun();
            var nextPosition = _arena.MoveTo(this, prey);
            Position = nextPosition;
            CurrentEnergy -= CanRun() ? RunEnergy() : WalkEnergy();
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
            return ActParametersProvider.GetStepFrequency();
        }

        return Speed;
    }

    private bool CanRun()
    {
        return Energy > 30;
    }
    
    private bool CanClone()
    {
        return Age is > CloneMinAge and < CloneMaxAge && CurrentEnergy > CloneMinEnergy;
    }

    private int TryToRun()
    {
        return CanRun()
            ? ActParametersProvider.GetStepFrequency() / 2
            : ActParametersProvider.GetStepFrequency();
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