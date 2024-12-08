using BotLife.Contracts;

namespace BotLife.Application.Bot;

public interface IActionBehaviour
{
    Activity GetAction(IBot bot, double currentEnergy, double energyThreshold, Activity last, IEnumerable<Event> events);
}