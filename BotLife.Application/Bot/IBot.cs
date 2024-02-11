namespace BotLife.Application.Bot;

public interface IBot
{
    Guid Id { get; }
    BotType Type { get; }
    double Energy { get; }
    int Age { get; }
    Position Position { get; }
    void SetPosition(Position position);
    void Next();
    bool IsAlive();
    void Rip();
}

public class EmptyBot : IBot
{
    public Guid Id { get; } = Guid.Empty;
    public BotType Type { get; } = BotType.None;
    public double Energy => 0;
    public int Age => 0;
    public Position Position { get; } = Position.Empty;
    public void SetPosition(Position position) { }
    public void Next() { }
    public bool IsAlive() => false;
    public void Rip() { }

    public static EmptyBot Instance => new();
}