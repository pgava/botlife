using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.Shared;
using BotLife.Contracts;
using MediatR;
using Serilog;

namespace BotLife.Application.Bot.Eta;

public class EtaBot(
    ILogger logger,
    IMediator mediator,
    IRandomizer randomizer,
    IArena arena,
    IBotActParametersProvider parametersProvider)
    : BotBase(logger, mediator, randomizer, arena,
        parametersProvider)
{
    public override BotType Type => BotType.Eta;
    public override IBot CreateBot()
    {
        return new EtaBot(Logger, Mediator, Randomizer, Arena, ActParametersProvider);
    }

    protected override Activity GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is a Mu in the area then try to catch it.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundMu) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Energy >= ActParametersProvider.GetEnergy()
                ? Activity.Trigger(Event.Empty(this, EmptyBot.Instance), ActivityType.WalkAround)
                : Activity.Trigger(@event, ActivityType.Catch),
            _ => Activity.Trigger(Event.Empty(this, EmptyBot.Instance), ActivityType.WalkAround)
        };

        // Keep catching the same bot.
        if (LastAction.Type == ActivityType.Catch && nextAction.Type == ActivityType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActivityType.Catch && nextAction.Type != ActivityType.Catch ||
            LastAction.Type != ActivityType.Catch)
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
    
    private Activity GetRandomAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // Get a random action from the list of events.
        var @event = eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance);
        
        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Activity.Trigger(@event, ActivityType.Catch),
            _ => Activity.Trigger(Event.Empty(this, EmptyBot.Instance), ActivityType.WalkAround)
        };
        
        // Keep catching the same bot.
        if (LastAction.Type == ActivityType.Catch && nextAction.Type == ActivityType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActivityType.Catch && nextAction.Type != ActivityType.Catch ||
            LastAction.Type != ActivityType.Catch)
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
