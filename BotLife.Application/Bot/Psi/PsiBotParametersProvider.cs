namespace BotLife.Application.Bot.Psi;

public class PsiBotParametersProvider : IBotParametersProvider
{
    public int GetEnergy() => 100;

    public int GetYearCycles() => 300;

    public int GetMaxAge() => 10;
    
    public int GetCloneMinEnergy() => 10;

    public int GetCloneMinAge() => 1;

    public int GetCloneMaxAge() => 100;
}