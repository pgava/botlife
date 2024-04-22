using Dapper;
using BotLife.Application.DataAccess;
using BotLife.Application.DataAccess.Models;
using BotLife.Application.Shared;
using MediatR;

namespace BotLife.Application.Bot.LogEvent;

public class LogEventCommand : IRequest
{
    public Act Action { get; }
    public double Energy { get; }
    public EventStatus Status { get; }

    public LogEventCommand(Act action, double energy, EventStatus status)
    {
        Action = action;
        Energy = energy;
        Status = status;
    }
    
}

public class LogEventCommandHandler : IRequestHandler<LogEventCommand>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IQueryProvider _queryProvider;
    
    public LogEventCommandHandler(ISqlConnectionFactory sqlConnectionFactory, TimeProvider timeProvider, IGuidGenerator guidGenerator, IQueryProvider queryProvider)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _timeProvider = timeProvider;
        _guidGenerator = guidGenerator;
        _queryProvider = queryProvider;
    }
    
    public async Task Handle(LogEventCommand request, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();
        if (connection == null)
        {
            return;
        }
        
        // Get pending events for the bot.
        var getEventsParams = new
        {
            BotId = request.Action.Event.From.Id, 
            Status = nameof(EventStatus.Pending), 
            EventType = request.Action.Event.Type
        };

        var events = await connection.QueryAsync<EventModel>(_queryProvider.GetEventQuery, getEventsParams);
        var eventModels = events.ToList();

        if (eventModels.Any())
        {
            // Set the status of the event to completed.
            var updateEventParams = new
            {
                Id = eventModels.First().Id, 
                Status = nameof(EventStatus.Completed), 
                Energy = request.Energy, 
                UpdatedAt = _timeProvider.GetLocalNow()
            };
            _ = await connection.ExecuteAsync(_queryProvider.UpdateEventQuery, updateEventParams);
        }
        
        if (request.Action.Type != Act.Empty.Type)
        {
            // Insert a new event.
            var insertEventParams = new
            {
                Id = _guidGenerator.NewGuid(), 
                BotId = request.Action.Event.From.Id, 
                BotType = request.Action.Event.From.Type, 
                Event = request.Action.Event.Type, 
                Action = request.Action.Type, 
                Energy = request.Energy, 
                Status = nameof(EventStatus.Pending), 
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _ = await connection.ExecuteAsync(_queryProvider.InsertEventQuery, insertEventParams);
        }
    }
}
