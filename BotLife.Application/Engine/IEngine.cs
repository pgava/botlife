using BotLife.Application.Models;

namespace BotLife.Application.Engine;

public interface IEngine
{
    void Start(
        int width = BotEngine.DefaultWidth,
        int height = BotEngine.DefaultHeight,
        int muBots = BotEngine.DefaultMuBots);
    IEnumerable<BotActor> Next();
    bool IsInitialized { get; }
}