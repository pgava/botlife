using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.Shared;
using BotLife.Contracts;
using MediatR;
using Serilog;

namespace BotLife.Application.Bot.Mu;

public class MuBot(
    ILogger logger,
    IMediator mediator,
    IRandomizer randomizer,
    IArena arena,
    IBotActParametersProvider parametersProvider)
    : BotBase(logger, mediator, randomizer, arena, parametersProvider)
{
    public override BotType Type => BotType.Mu;
    public override IBot CreateBot()
    {
        return new MuBot(Logger, Mediator, Randomizer, Arena, ActParametersProvider);
    }

    protected override Activity GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is an Eta in the area then try to escape.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundEta) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundPsi => Energy >= ActParametersProvider.GetEnergy()
                ? Activity.Trigger(Event.Empty(this, EmptyBot.Instance), ActivityType.WalkAround)
                : Activity.Trigger(@event, ActivityType.Catch),
            EventType.FoundEta => Activity.Trigger(@event, ActivityType.Escape),
            _ => Activity.Trigger(Event.Empty(this, EmptyBot.Instance), ActivityType.WalkAround)
        };

        // Keep catching the same bot
        if (LastAction.Type == ActivityType.Catch && nextAction.Type == ActivityType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActivityType.Escape && nextAction.Type != ActivityType.Escape ||
            LastAction.Type != ActivityType.Escape)
        {
            Speed = ActParametersProvider.GetStepFrequency();
        }

        if (LastAction.Type != nextAction.Type)    
        {
            // Log the event.
            Mediator.Send(new LogEventCommand(nextAction, CurrentEnergy, EventStatus.Pending));
        }
        
        return nextAction;
    }
    
}
