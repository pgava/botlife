using BotLife.Contracts;

namespace BotLife.Application.Arena;

public class CollisionManager : ICollisionManager
{
    public bool CanCollide(IBot bot, List<IBot> others)
    {
        return others.All(b => b.Type != BotType.Wall);
    }
}