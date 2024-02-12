﻿
using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.MuBot;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddTransient<IArena, BotArena>();
        services.AddTransient<ICollisionManager, CollisionManager>();
        services.AddMediatR(config =>
        {
            // Register all handlers from the BotLife.Application assembly.
            // Use RegisterServicesFromAssemblies to register handlers from multiple assemblies.
            config.RegisterServicesFromAssembly(typeof(BotArena).Assembly);
        });
    })
    .Build();

Console.WriteLine("Bot Life Starting...");

var arena = new BotArena(host.Services.GetService<ICollisionManager>()!);
arena.BuildArena(64, 48);
var bots = new List<IBot>();

for (var count = 0; count < 10; count++)
{
    var bot = new MuBot(host.Services.GetService<IMediator>()!, arena, new MuBotActParametersProvider());
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



