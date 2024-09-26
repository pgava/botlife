using BotLife.Application.Bot;

namespace BotLife.Application.Tests;

public class FakeParametersProvider : IBotParametersProvider
{
    public int GetEnergy() => 100;

    public int GetYearCycles() => 10;

    public int GetMaxAge() => 10;
    
    public int GetCloneMinEnergy() => 10;

    public int GetCloneMinAge() => 1;

    public int GetCloneMaxAge() => 100;

}