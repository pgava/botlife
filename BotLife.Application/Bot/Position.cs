namespace BotLife.Application.Bot;

public record Position(int X, int Y)
{
    public static Position Empty => new(-1, -1);
    public static Position At(int x, int y) => new(x, y);
}