using BotLife.Api.Services;
using BotLife.Application.Arena;
using BotLife.Application.Engine;
using BotLife.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BotLife.Api;

public static class BotLifeApi
{
    public static void MapBotLifeApi(this WebApplication app)
    {
        app.MapPost("/start", Start);
        app.MapGet("/get-next", GetNext);
    }

    public static void AddBotLifeServices(this IServiceCollection services)
    {
        services.AddSingleton<IRandomizer, Randomizer>();
        services.AddSingleton<ICollisionManager, CollisionManager>();
        services.AddSingleton<IArena, BotArena>();
        services.AddSingleton<IEngine, BotEngine>();
        services.AddSingleton<IBotLifeService, BotLifeService>();
        services.AddMediatR(config =>
        {
            // Register all handlers from the BotLife.Application assembly.
            // Use RegisterServicesFromAssemblies to register handlers from multiple assemblies.
            config.RegisterServicesFromAssembly(typeof(BotEngine).Assembly);
        });
    }

    public static IResult Start([FromServices] IBotLifeService botLifeService)
    {
        botLifeService.Start();
        return TypedResults.Ok();
    }

    public static IResult GetNext([FromServices] IBotLifeService botLifeService)
    {
        return TypedResults.Ok(botLifeService.Next());
    }
}