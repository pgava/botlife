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
    private IMediator Mediator { get; } = new Mock<IMediator>().Object;
    private ILogger Logger { get; } = new Mock<ILogger>().Object;
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

    private IEngine CreateEngine()
    {
        return new BotEngine(Logger, Mediator, Randomizer(), Arena);
    }

    private IArena CreateArena()
    {
        var arena = new BotArena(new CollisionManager());
        arena.BuildArena(10, 10);
        return arena;
    }

}