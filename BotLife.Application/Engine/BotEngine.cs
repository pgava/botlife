using System.Collections.Concurrent;
using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.MuBot;
using BotLife.Application.Bot.PsiBot;
using BotLife.Application.Models;
using BotLife.Application.Shared.Exceptions;

namespace BotLife.Application.Engine;

public class BotEngine : IEngine
{
    private readonly IArena _arena;
    private readonly ConcurrentDictionary<Guid, IBot> _bots = new();
    private bool _isInitialized;

    // Cycle is used to track the number of iteration the engine has run.
    // Assume that the cycles increases regularly at a frequency of 10 times per second.
    // The cycle is used to track:
    // - The age of the bots.
    // - The speed of the bots. For example a bot normal speed would be 1 step per 10 cycles.
    private int _cycle = 0;

    public const int DefaultWidth = 64;
    public const int DefaultHeight = 48;
    public const int DefaultMuBots = 10;
    public const int DefaultPsiBots = 20;

    public bool IsInitialized => _isInitialized;
    public BotEngine(IArena arena)
    {
        _arena = arena;
    }

    public void Start(int width = DefaultWidth,
        int height = DefaultHeight,
        int muBots = DefaultMuBots)
    {
        _arena.BuildArena(width, height);
        _bots.Clear();

        for (var countMuBot = 0; countMuBot < muBots; countMuBot++)
        {
            var muBot = new MuBot(_arena, new MuBotActParametersProvider());
            _arena.AddBotAtRandom(muBot);
            _bots.TryAdd(muBot.Id, muBot);
        }

        Enumerable.Range(0, DefaultPsiBots)
            .Select(_ => new PsiBot(new PsiBotParametersProvider()))
            .ToList()
            .ForEach(bot =>
            {
                _arena.AddBotAtRandom(bot);
                _bots.TryAdd(bot.Id, bot);
            });

        _isInitialized = true;
    }

    public IEnumerable<BotActor> Next()
    {
        EngineNotInitializedException.ThrowIfNotInitialized(this, "Engine not started.");

        _cycle++;

        foreach (var bot in _bots)
        {
            bot.Value.Next();

            if (!bot.Value.IsAlive())
            {
                _arena.RemoveBot(bot.Value);
                _bots.TryRemove(bot.Key, out _);
            }
        }
        
        return GetActors();
    }

    private IEnumerable<BotActor> GetActors()
    {
        return _bots.Select(bot => new BotActor
        {
            Name = bot.Value.Id.ToString(),
            Type = bot.Value.Type,
            Age = bot.Value.Age,
            Energy = bot.Value.Energy,
            Position = bot.Value.Position
        });
    }

}