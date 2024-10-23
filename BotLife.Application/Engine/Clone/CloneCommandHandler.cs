using BotLife.Application.Bot;
using BotLife.Contracts;
using MediatR;

namespace BotLife.Application.Engine.Clone;

public class CloneCommand : IRequest
{
    public CloneCommand(IBot bot)
    {
        Bot = bot;
    }

    public IBot Bot { get; }
}

public class CloneCommandHandler : IRequestHandler<CloneCommand>
{
    private readonly IEngine _engine;

    public CloneCommandHandler(IEngine engine)
    {
        _engine = engine;
    }

    public Task Handle(CloneCommand request, CancellationToken cancellationToken)
    {
        _engine.Clone(request.Bot);
        return Task.CompletedTask;
    }
}