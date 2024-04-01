namespace BotLife.Application.DataAccess.Models;

public enum EventStatus
{
    Pending,
    Completed
}

public record EventModel(
    Guid Id,
    Guid BotId,
    string BotType,
    string EventType,
    string ActionType,
    decimal EnergyBegin,
    decimal? EnergyEnd,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);


