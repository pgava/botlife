using BotLife.Application.Engine.Clone;
using BotLife.Application.Shared;
using MediatR;

namespace BotLife.Application.Bot.PsiBot;

public class PsiBot : IBot
{
    private readonly IBotParametersProvider _parametersProvider;
    private readonly IMediator _mediator;
    private readonly IRandomizer _randomizer;
    private double _energy;
    private int _cycle;
    private const int CloneMinAge = 0;
    private const int CloneMinEnergy = 10;

    public Guid Id { get; }  = Guid.NewGuid();
    public BotType Type { get; } = BotType.PsiBot;
    public double Energy { get; }
    public int Age => _cycle / _parametersProvider.GetAgeFactor();
    public Position Position { get; private set; } = Position.Empty;


    public PsiBot(IMediator mediator, IRandomizer randomizer,  IBotParametersProvider parametersProvider)
    {
        _parametersProvider = parametersProvider;
        _mediator = mediator;
        _randomizer = randomizer;
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
        if (Age > CloneMinAge && _energy > CloneMinEnergy)
        {
            // Avoid cloning all at the same time.
            var nextGeneration = _randomizer.Rnd(0, 50);

            // New generation twice a year.
            if (_cycle % (_parametersProvider.GetAgeFactor() / 2 + nextGeneration) != 0) return;

            _mediator.Send(new CloneCommand(new PsiBot(_mediator, _randomizer, _parametersProvider)));
        }
    }

    public double CycleEnergyLoss()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.2, 4);
    }
}