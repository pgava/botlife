using BotLife.Application.Models;

namespace BotLife.Api.Services;

public interface IBotLifeService
{
    void Start();
    IEnumerable<BotActor> Next();
}