using BotLife.Application.Bot;

namespace BotLife.Application.Shared.Exceptions;

public class ArenaSpaceNotMatchException : Exception
{
    public ArenaSpaceNotMatchException()
    {
    }

    public ArenaSpaceNotMatchException(string message)
        : base(message)
    {
    }

    public ArenaSpaceNotMatchException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfNotMatch(int free, int available, string message)
    {
        if (free != available)
        {
            throw new ArenaSpaceNotMatchException(message);
        }
    }
}