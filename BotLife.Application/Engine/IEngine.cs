using BotLife.Application.Bot;
using BotLife.Application.Models;

namespace BotLife.Application.Engine;

public interface IEngine
{
    void Start(
        int width = BotEngine.DefaultWidth,
        int height = BotEngine.DefaultHeight);
    IEnumerable<BotActor> Next();
    bool IsInitialized { get; }
    void Clone(IBot bot);
}