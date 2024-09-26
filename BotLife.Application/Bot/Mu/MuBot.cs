using BotLife.Application.Arena;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.DataAccess.Models;
using BotLife.Application.Shared;
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

    protected override Act GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is a Eta in the area then try to escape.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundEta) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(this, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundPsi => Energy >= ActParametersProvider.GetEnergy()
                ? Act.Trigger(Event.Empty(this, EmptyBot.Instance), ActType.WalkAround)
                : Act.Trigger(@event, ActType.Catch),
            EventType.FoundEta => Act.Trigger(@event, ActType.Escape),
            _ => Act.Trigger(Event.Empty(this, EmptyBot.Instance), ActType.WalkAround)
        };

        // Keep catching the same bot
        if (LastAction.Type == ActType.Catch && nextAction.Type == ActType.Catch)
        {
            return LastAction;
        }

        // If bot escaped then stop running.
        if (LastAction.Type == ActType.Escape && nextAction.Type != ActType.Escape ||
            LastAction.Type != ActType.Escape)
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
