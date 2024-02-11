using BotLife.Application.Arena;

namespace BotLife.Application.Shared.Exceptions;

public class ArenaNotInitializedException : Exception
{
    public ArenaNotInitializedException()
    {
    }

    public ArenaNotInitializedException(string message)
        : base(message)
    {
    }

    public ArenaNotInitializedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfNotInitialized(IArena obj, string message)
    {
        if (!obj.IsInitialized)
        {
            throw new ArenaNotInitializedException(message);
        }
    }
}

