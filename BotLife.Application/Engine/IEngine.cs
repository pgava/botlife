using BotLife.Application.Models;
using BotLife.Contracts;

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