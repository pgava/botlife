namespace BotLife.Application.Bot.LogEvent;

public interface IQueryProvider
{
    string GetEventQuery { get; }
    string UpdateEventQuery { get; }
    string InsertEventQuery { get; }
}

public class QueryProvider : IQueryProvider
{
    public string GetEventQuery => "SELECT TOP 1 * " +
                                   "FROM [Event] AS [e] " +
                                   "WHERE [e].[BotId] = @BotId AND " +
                                   "  [e].[Status] = @Status AND " +
                                   "  [e].[EventType]= @EventType";

    public string UpdateEventQuery => "UPDATE [Event] " +
                                      "SET [Status] = @Status, " +
                                      "[EnergyEnd] = @Energy, " +
                                      "[UpdatedAt] = @UpdatedAt " +
                                      "WHERE [Id] = @Id";

    public string InsertEventQuery => "INSERT INTO [Event] " +
                                      "([Id], [BotId], [BotType], [EventType], [ActionType], [EnergyBegin], [Status], [CreatedAt]) " +
                                      "VALUES (@Id, @BotId, @BotType, @Event, @Action, @Energy, @Status, @CreatedAt)";
}