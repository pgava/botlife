using BotLife.Application.DataAccess.Models;
using BotLife.Contracts;
using BotLife.DataAccess;
using Dapper;
using MassTransit;

namespace BotLife.Consumer;

public class LogEventConsumer : IConsumer<EventContract>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IQueryProvider _queryProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidGenerator _guidGenerator;

    public LogEventConsumer(ISqlConnectionFactory sqlConnectionFactory, IQueryProvider queryProvider, TimeProvider timeProvider, IGuidGenerator guidGenerator)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _queryProvider = queryProvider;
        _timeProvider = timeProvider;
        _guidGenerator = guidGenerator;
    }
    
    public async Task Consume(ConsumeContext<EventContract> context)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();
        if (connection == null)
        {
            return;
        }
        
        // Get pending events for the bot.
        var getEventsParams = new
        {
            BotId = context.Message.FromBotId, 
            Status = nameof(EventStatus.Pending)
        };

        var events = await connection.QueryAsync<EventModel>(_queryProvider.GetEventQuery, getEventsParams);
        var eventModels = events.ToList();

        if (eventModels.Any())
        {
            // Set the status of the event to complete.
            var updateEventParams = new
            {
                eventModels.First().Id, 
                Status = nameof(EventStatus.Completed),
                context.Message.Energy, 
                UpdatedAt = _timeProvider.GetLocalNow()
            };
            _ = await connection.ExecuteAsync(_queryProvider.UpdateEventQuery, updateEventParams);
        }
        
        if (context.Message.Action != Activity.Empty(EmptyBot.Instance, EmptyBot.Instance).Type)
        {
            // Insert a new event.
            var insertEventParams = new
            {
                Id = _guidGenerator.NewGuid(), 
                BotId = context.Message.FromBotId,   
                BotType = context.Message.FromBotType.ToString(), 
                Event = context.Message.EventType.ToString(), 
                Action = context.Message.Action.ToString(),
                context.Message.Energy, 
                Status = nameof(EventStatus.Pending), 
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _ = await connection.ExecuteAsync(_queryProvider.InsertEventQuery, insertEventParams);
        }
    }
}