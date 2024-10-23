namespace BotLife.Contracts;

public interface IGuidGenerator
{
    Guid NewGuid();
}

public class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}

