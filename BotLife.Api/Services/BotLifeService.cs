using BotLife.Application.Engine;
using BotLife.Application.Models;

namespace BotLife.Api.Services;

public class BotLifeService : IBotLifeService
{
    private readonly IEngine _engine;

    public BotLifeService(IEngine engine)
    {
        _engine = engine;
    }
    
    public void Start()
    {
        _engine.Start();
    }

    public IEnumerable<BotActor> Next()
    {
        return _engine.Next();
    }
}