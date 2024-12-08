using BotLife.Contracts;

namespace BotLife.Application.Bot.Eta;

public interface IEtaActionBehaviour : IActionBehaviour;

public class EtaBestAction : IEtaActionBehaviour
{
    public Activity GetAction(IBot bot, double currentEnergy, double energyThreshold, Activity last, IEnumerable<Event> events)
    {
        var eventList = events.ToList();

        // If there is a Mu in the area then try to catch it.
        var @event = eventList.FirstOrDefault(e => e.Type == EventType.FoundMu) ??
                     (eventList.FirstOrDefault() ?? Event.Empty(bot, EmptyBot.Instance));

        var nextAction = @event.Type switch
        {
            EventType.FoundMu => currentEnergy >= energyThreshold
                ? Activity.Trigger(Event.Empty(bot, EmptyBot.Instance), ActivityType.WalkAround)
                : Activity.Trigger(@event, ActivityType.Catch),
            _ => Activity.Trigger(Event.Empty(bot, EmptyBot.Instance), ActivityType.WalkAround)
        };

        // Keep catching the same bot.
        if (last.Type == ActivityType.Catch && nextAction.Type == ActivityType.Catch)
        {
            return last;
        }

        return nextAction;
    }
}