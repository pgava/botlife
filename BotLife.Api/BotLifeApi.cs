using BotLife.Api.Services;
using BotLife.Application.Arena;
using BotLife.Application.Engine;
using BotLife.Consumer;
using BotLife.Contracts;
using BotLife.DataAccess;
using Microsoft.AspNetCore.Mvc;
using IQueryProvider = BotLife.Consumer.IQueryProvider;

namespace BotLife.Api;

public static class BotLifeApi
{
    public static void MapBotLifeApi(this WebApplication app)
    {
        app.MapPost("/start", Start);
        app.MapGet("/get-next", GetNext);
    }

    public static void AddBotLifeServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        services.AddSingleton<IQueryProvider, QueryProvider>();
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
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>(_ =>
        {
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("BotLifeDb");

            // Return new SqlConnectionFactory with the connection string
            return new SqlConnectionFactory(connectionString);
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