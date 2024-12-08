using BotLife.Contracts;

namespace BotLife.Application.Bot.Mu;

public interface IMuActionBehaviour : IActionBehaviour;

public class MuBestAction : IMuActionBehaviour
{
    public Activity GetAction(IBot bot, double currentEnergy, double energyThreshold, Activity last, IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is an Eta in the area then try to escape.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundEta) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(bot, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundPsi => currentEnergy >= energyThreshold
                ? Activity.Trigger(Event.Empty(bot, EmptyBot.Instance), ActivityType.WalkAround)
                : Activity.Trigger(@event, ActivityType.Catch),
            EventType.FoundEta => Activity.Trigger(@event, ActivityType.Escape),
            _ => Activity.Trigger(Event.Empty(bot, EmptyBot.Instance), ActivityType.WalkAround)
        };

        // Keep catching the same bot
        if (last.Type == ActivityType.Catch && nextAction.Type == ActivityType.Catch)
        {
            return last;
        }

        return nextAction;
    }
}