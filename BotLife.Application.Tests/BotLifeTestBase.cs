using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.MuBot;
using BotLife.Application.Engine;
using MediatR;
using Moq;

namespace BotLife.Application.Tests;

public class BotLifeTestBase
{
    protected IMediator Mediator { get; } = new Mock<IMediator>().Object;
    protected IArena Arena { get; }
    protected IEngine Engine { get; }

    public BotLifeTestBase()
    {
        ICollisionManager collisionManager = new CollisionManager();
        Arena = new BotArena(collisionManager);
        Arena.BuildArena(10, 10);
        Engine = new BotEngine(Mediator, Arena);
    }

    public virtual MuBot CreateMuBot()
    {
        return new MuBot(Mediator, Arena, new FakeActParametersProvider());
    }
}