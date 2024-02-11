using BotLife.Application.Arena;
using BotLife.Application.Engine;

namespace BotLife.Application.Tests;

public class BotLifeTestHelper
{
    private readonly ICollisionManager _collisionManager;
    public IArena Arena { get; }
    public IEngine Engine { get; }

    public BotLifeTestHelper()
    {
        _collisionManager = new CollisionManager();
        Arena = new BotArena(_collisionManager);
        Arena.BuildArena(10, 10);
        Engine = new BotEngine(Arena);
    }
}