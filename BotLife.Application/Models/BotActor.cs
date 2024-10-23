using BotLife.Contracts;

namespace BotLife.Application.Models;

public record BotActor
{
    public string Name { get; set; } = default!;
    public BotType Type { get; set; }
    public int Age { get; set; }
    public double Energy { get; set; }
    public Position Position { get; set; } = Position.Empty;
}