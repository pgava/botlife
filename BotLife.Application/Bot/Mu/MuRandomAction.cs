using BotLife.Contracts;

namespace BotLife.Application.Bot.Eta;


public class MuRandomAction : IEtaActionBehaviour
{
    public Activity GetAction(IBot bot, double currentEnergy, double energyThreshold, Activity last, IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // Get a random action from the list of events.
        var @event = eventList.FirstOrDefault() ?? Event.Empty(bot, EmptyBot.Instance);

        var nextAction = @event.Type switch
        {
            EventType.FoundMu => Activity.Trigger(@event, ActivityType.Catch),
            EventType.FoundPsi => Activity.Trigger(@event, ActivityType.Catch),
            _ => Activity.Trigger(Event.Empty(bot, EmptyBot.Instance), ActivityType.WalkAround)
        };

        return nextAction;
    }
}