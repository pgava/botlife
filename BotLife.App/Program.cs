using BotLife.Application.Arena;
using BotLife.Application.Bot.Mu;
using BotLife.Application.Shared;
using BotLife.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddTransient<IArena, BotArena>();
        services.AddTransient<ICollisionManager, CollisionManager>();
        services.AddTransient<IRandomizer, Randomizer>();
        services.AddMediatR(config =>
        {
            // Register all handlers from the BotLife.Application assembly.
            // Use RegisterServicesFromAssemblies to register handlers from multiple assemblies.
            config.RegisterServicesFromAssembly(typeof(BotArena).Assembly);
        });
    })
    .UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration))
    .Build();

Console.WriteLine("Bot Life Starting...");

var arena = new BotArena(host.Services.GetService<ICollisionManager>()!);
arena.BuildArena(80, 60);
var bots = new List<IBot>();

for (var count = 0; count < 10; count++)
{
    var bot = new MuBot(
        host.Services.GetService<ILogger>()!,
        host.Services.GetService<IMediator>()!,
        host.Services.GetService<IRandomizer>()!,
        arena,
        new MuBestAction(),
        new MuBotActParametersProvider());
    bots.Add(bot);
    arena.AddBotAtRandom(bot);
}

Console.Write("\nPress 'Space' to exit the game...");
while (Console.ReadKey().Key != ConsoleKey.Spacebar)
{
    // For each bot, scan the area around it and react to the events.
    foreach (var bot in bots)
    {
        bot.Next();
    }
}

Console.WriteLine("\nBot Life Ending...");




