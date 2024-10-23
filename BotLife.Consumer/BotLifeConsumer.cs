using BotLife.Contracts;
using BotLife.DataAccess;

namespace BotLife.Consumer;

public static class BotLifeConsumer
{
    public static void AddBotLifeServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        services.AddSingleton<IQueryProvider, QueryProvider>();
        services.AddSingleton<IRandomizer, Randomizer>();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>(_ =>
        {
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("BotLifeDb");

            // Return new SqlConnectionFactory with the connection string
            return new SqlConnectionFactory(connectionString);
        });
    }
}