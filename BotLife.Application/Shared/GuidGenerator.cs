namespace BotLife.Application.Shared;

public interface IGuidGenerator
{
    Guid NewGuid();
}

public class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}

