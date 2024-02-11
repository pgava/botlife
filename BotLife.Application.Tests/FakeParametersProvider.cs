using BotLife.Application.Bot;

namespace BotLife.Application.Tests;

public class FakeParametersProvider : IBotParametersProvider
{
    public int GetEnergy() => 100;

    public int GetAgeFactor() => 10;
}