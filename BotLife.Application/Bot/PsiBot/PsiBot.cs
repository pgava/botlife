namespace BotLife.Application.Bot.PsiBot;

public class PsiBot : IBot
{
    private readonly IBotParametersProvider _parametersProvider;
    private double _energy;
    private int _cycle;
    public Guid Id { get; }  = Guid.NewGuid();
    public BotType Type { get; } = BotType.PsiBot;
    public double Energy { get; }
    public int Age => _cycle / _parametersProvider.GetAgeFactor();
    public Position Position { get; private set; } = Position.Empty;


    public PsiBot(IBotParametersProvider parametersProvider)
    {
        _parametersProvider = parametersProvider;
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
        if (Age > 3 && _energy > 10)
        {

        }

    }

    public double CycleEnergyLoss()
    {
        return Math.Round(Math.Log(Math.Max(Age, 1) + 1) * 0.2, 4);
    }
}