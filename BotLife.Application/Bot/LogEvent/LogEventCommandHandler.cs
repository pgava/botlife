using BotLife.Contracts;
using MassTransit;
using MediatR;

namespace BotLife.Application.Bot.LogEvent;

public class LogEventCommand : IRequest
{
    public EventContract EventContract { get; }
    public LogEventCommand(Activity action, double energy, EventStatus status)
    {
        EventContract = new EventContract(action.Type, action.Event.From.Id, action.Event.From.Type,
            action.Event.To.Id, action.Event.To.Type,
            energy, action.Event.Type, status);
    }
}

public class LogEventCommandHandler : IRequestHandler<LogEventCommand>
{
    private readonly IBus _bus;

    public LogEventCommandHandler(IBus bus)
    {
        _bus = bus;
    }
    
    public async Task Handle(LogEventCommand request, CancellationToken cancellationToken)
    {
        await _bus.Publish(request.EventContract, cancellationToken);
    }
}
