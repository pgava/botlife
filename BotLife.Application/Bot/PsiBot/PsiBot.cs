using BotLife.Application.Engine.Clone;
using MediatR;

namespace BotLife.Application.Bot.PsiBot;

public class PsiBot : IBot
{
    private readonly IBotParametersProvider _parametersProvider;
    private readonly IMediator _mediator;
    private double _energy;
    private int _cycle;
    public Guid Id { get; }  = Guid.NewGuid();
    public BotType Type { get; } = BotType.PsiBot;
    public double Energy { get; }
    public int Age => _cycle / _parametersProvider.GetAgeFactor();
    public Position Position { get; private set; } = Position.Empty;


    public PsiBot(IMediator mediator, IBotParametersProvider parametersProvider)
    {
        _parametersProvider = parametersProvider;
        _mediator = mediator;
        _energy = _parametersProvider.GetEnergy();
    }

    public void SetPosition(Position position)
    {
        Position = position;
    }

    public void Next()
    {
        _cycle++;
        _energy -= CycleEnergyLoss();
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

    public void Clone()
    {
        if (Age > 0 && _energy > 10)
        {
            // Get a random number between 0 and 100.
            // Avoid cloning all at the same time.
            var nextGeneration = new Random().Next(0, 50);
            if (_cycle % (_parametersProvider.GetAgeFactor() / 2 + nextGeneration) != 0) return;

            _mediator.Send(new CloneCommand(new PsiBot(_mediator, _parametersProvider)));
        }
    }

    public double CycleEnergyLoss()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.2, 4);
    }
}