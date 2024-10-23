using BotLife.Contracts;

namespace BotLife.Application.Shared.Exceptions;

public class ArenaFullException : Exception
{
    public ArenaFullException()
    {
    }

    public ArenaFullException(string message)
        : base(message)
    {
    }

    public ArenaFullException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfFull(List<Position> obj, string message)
    {
        if (obj.Count == 0)
        {
            throw new ArenaFullException(message);
        }
    }
}