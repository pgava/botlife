using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.DataAccess.Models;
using BotLife.Application.Shared;
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

    protected override Act GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is a Mu in the area then try to catch it.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundMu) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Energy >= ActParametersProvider.GetEnergy()
                ? Act.Trigger(Event.Empty(this, EmptyBot.Instance), ActType.WalkAround)
                : Act.Trigger(@event, ActType.Catch),
            _ => Act.Trigger(Event.Empty(this, EmptyBot.Instance), ActType.WalkAround)
        };

        // Keep catching the same bot.
        if (LastAction.Type == ActType.Catch && nextAction.Type == ActType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActType.Catch && nextAction.Type != ActType.Catch ||
            LastAction.Type != ActType.Catch)
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
    
    private Act GetRandomAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // Get a random action from the list of events.
        var @event = eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance);
        
        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Act.Trigger(@event, ActType.Catch),
            _ => Act.Trigger(Event.Empty(this, EmptyBot.Instance), ActType.WalkAround)
        };
        
        // Keep catching the same bot.
        if (LastAction.Type == ActType.Catch && nextAction.Type == ActType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActType.Catch && nextAction.Type != ActType.Catch ||
            LastAction.Type != ActType.Catch)
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
