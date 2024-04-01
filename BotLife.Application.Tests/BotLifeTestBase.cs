using Microsoft.Extensions.Time.Testing;
using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.Mu;
using BotLife.Application.Bot.Psi;
using BotLife.Application.Engine;
using BotLife.Application.Shared;
using MediatR;
using Moq;
using Serilog;

namespace BotLife.Application.Tests;

public class BotLifeTestBase
{
    protected IMediator Mediator { get; } = new Mock<IMediator>().Object;
    protected ILogger Logger { get; } = new Mock<ILogger>().Object;
    protected IGuidGenerator GuidGenerator => GuidGeneratorMock.Object;
    protected Mock<IGuidGenerator> GuidGeneratorMock { get; } = new();
    protected FakeTimeProvider TimeProvider { get; } = new ();
    protected IArena Arena { get; }
    protected IEngine Engine { get; }

    protected BotLifeTestBase()
    {
        Arena = CreateArena();
        Engine = CreateEngine();
    }

    protected virtual IBotActParametersProvider ActParameters()
    {
        return new FakeActParametersProvider();
    }

    protected virtual IBotParametersProvider Parameters()
    {
        return new FakeParametersProvider();
    }

    protected virtual IRandomizer Randomizer()
    {
        return new Randomizer();
    }

    protected virtual MuBot CreateMuBot()
    {
        return new MuBot(Logger, Mediator, Randomizer(), Arena, ActParameters());
    }

    protected virtual PsiBot CreatePsiBot()
    {
        return new PsiBot(Logger, Mediator, Randomizer(), Parameters());
    }

    // Set up guid generator
    protected virtual void SetupGuidGenerator()
    {
        GuidGeneratorMock.Setup(m => m.NewGuid()).Returns(Guid.NewGuid);
    }
    
    // Set up time provider
    protected virtual void SetupTimeProvider()
    {
        TimeProvider.SetUtcNow(DateTime.Parse("2024-05-01"));
        TimeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
    }

    protected MuBot AddMuBotAt(Position from)
    {
        var bot = CreateMuBot();
        Arena.AddBotAtPosition(bot, from);
        return bot;
    }

    protected PsiBot AddPsiBotAt(Position from)
    {
        var bot = CreatePsiBot();
        Arena.AddBotAtPosition(bot, from);
        return bot;
    }

    private IEngine CreateEngine()
    {
        return new BotEngine(Logger, Mediator, Randomizer(), Arena);
    }

    private IArena CreateArena()
    {
        var arena = new BotArena(new CollisionManager());
        arena.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        return arena;
    }
    
}

public static class TestConstants
{
    public const int MaxHeight = 30;
    public const int MaxWidth = 50;
}
