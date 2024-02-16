using BotLife.Application.Engine;

namespace BotLife.Application.Tests;

public class BotEngineTests : BotLifeTestBase
{
    private readonly IEngine _sut;

    public BotEngineTests()
    {
        _sut = Engine;
    }
    [Fact]
    public void Engine_Should_Start()
    {
        _sut.Start();
    }

    [Fact]
    public void After_Engine_Start_Should_Be_Able_To_Call_Next()
    {
        _sut.Start();
        _sut.Next();
    }
}

