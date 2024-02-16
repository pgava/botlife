using BotLife.Application.Bot;

namespace BotLife.Application.Tests;

public class FakeActParametersProvider : IBotActParametersProvider
{
    public int GetEnergy() => 100;

    public int GetScanArea() => 3;

    public int GetStepFrequency() => 2;

    public int GetYearCycles() => 300;

    public int GetMaxAge() => 5;

}