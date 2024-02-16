using System.Collections.Concurrent;
using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.Eta;
using BotLife.Application.Bot.Mu;
using BotLife.Application.Bot.Psi;
using BotLife.Application.Models;
using BotLife.Application.Shared;
using BotLife.Application.Shared.Exceptions;
using MediatR;
using Serilog;

namespace BotLife.Application.Engine;

public class BotEngine : IEngine
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IRandomizer _randomizer;
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
    public const int DefaultMuBots = 20;
    public const int DefaultEtaBots = 7;
    public const int DefaultPsiBots = 40;

    public bool IsInitialized => _isInitialized;

    public BotEngine(ILogger logger, IMediator mediator, IRandomizer randomizer, IArena arena)
    {
        _logger = logger;
        _mediator = mediator;
        _randomizer = randomizer;
        _arena = arena;
    }

    public void Start(int width = DefaultWidth,
        int height = DefaultHeight)
    {
        _arena.BuildArena(width, height);
        _bots.Clear();

        AddBots(DefaultMuBots,
            () => new MuBot(_logger, _mediator, _randomizer, _arena, new MuBotActParametersProvider()));

        AddBots(DefaultPsiBots,
            () => new PsiBot(_logger, _mediator, _randomizer, new PsiBotParametersProvider()));

        AddBots(DefaultEtaBots,
            () => new EtaBot(_logger, _mediator, _randomizer, _arena, new MuBotActParametersProvider()));

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

    public void Clone(IBot bot)
    {
        _arena.AddBotAtRandom(bot);
        _bots.TryAdd(bot.Id, bot);
    }

    private void AddBots(int n, Func<IBot> createBot)
    {
        Enumerable.Range(0, n)
            .Select(_ => createBot())
            .ToList()
            .ForEach(bot =>
            {
                _arena.AddBotAtRandom(bot);
                _bots.TryAdd(bot.Id, bot);
            });
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