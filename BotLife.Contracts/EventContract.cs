namespace BotLife.Contracts;

public record EventContract(ActivityType Action, Guid FromBotId, BotType FromBotType, 
    Guid ToBotId, BotType ToBotType, double Energy, EventType EventType, EventStatus EventStatus)
{
    public ActivityType Action { get; } = Action;
    public Guid FromBotId { get; } = FromBotId;
    public BotType FromBotType { get; } = FromBotType;
    public Guid ToBotId { get; } = ToBotId;
    public BotType ToBotType { get; } = ToBotType;

    public double Energy { get; } = Energy;
    public EventStatus EventStatus { get; } = EventStatus;
    public EventType EventType { get; } = EventType;
}