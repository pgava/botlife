using BotLife.Application.Arena;
using BotLife.Contracts;
using MediatR;
using Serilog;

namespace BotLife.Application.Bot.Mu;

public class MuBot(
    ILogger logger,
    IMediator mediator,
    IRandomizer randomizer,
    IArena arena,
    IMuActionBehaviour actionBehaviour, 
    IBotActParametersProvider parametersProvider)
    : BotBase(logger, mediator, randomizer, arena, parametersProvider)
{
    private IMuActionBehaviour ActionBehaviour { get; } = actionBehaviour;
    
    public override BotType Type => BotType.Mu;
    public override IBot CreateBot()
    {
        return new MuBot(Logger, Mediator, Randomizer, Arena, ActionBehaviour, ActParametersProvider);
    }

    protected override Activity GetBestAction(IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        return ActionBehaviour.GetAction(this, CurrentEnergy, ActParametersProvider.GetEnergy(), LastAction, eventList);
    }
    
}
