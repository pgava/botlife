using BotLife.Application.Bot;
using BotLife.Contracts;

namespace BotLife.Application.Arena;

public interface ICollisionManager
{
    /// <summary>
    /// Check if the current bot can collide with other bots.
    /// </summary>
    /// <returns>True or False.</returns>
    bool CanCollide(IBot bot, List<IBot> others);
}