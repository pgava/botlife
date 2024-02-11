using BotLife.Application.Engine;

namespace BotLife.Application.Shared.Exceptions;

public class EngineNotInitializedException : Exception
{
    public EngineNotInitializedException()
    {
    }

    public EngineNotInitializedException(string message)
        : base(message)
    {
    }

    public EngineNotInitializedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfNotInitialized(IEngine obj, string message)
    {
        if (!obj.IsInitialized)
        {
            throw new EngineNotInitializedException(message);
        }
    }
}