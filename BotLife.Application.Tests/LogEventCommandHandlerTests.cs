using BotLife.Application.Bot;
using BotLife.Application.Bot.LogEvent;
using BotLife.Application.DataAccess.Models;
using Moq;

namespace BotLife.Application.Tests;

public class LogEventCommandHandlerTests : BotLifeTestDbBase
{

    [Fact]
    public async Task Handle_WhenEventExists_UpdatesEvent()
    {
        // Arrange
        SetupMocks();
        SetupDapper(GetEventModelList());
        
        var command = CreateEmptyCommand();
        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        QueryProviderMock.Verify(q => q.GetEventQuery, Times.Once);
        QueryProviderMock.Verify(q => q.InsertEventQuery, Times.Never);
        QueryProviderMock.Verify(q => q.UpdateEventQuery, Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_InsertsNewEvent()
    {
        // Arrange
        SetupMocks();
        SetupDapper();
            
        var command = CreateCommand();
        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        QueryProviderMock.Verify(q => q.GetEventQuery, Times.Once);
        QueryProviderMock.Verify(q => q.InsertEventQuery, Times.Once);
        QueryProviderMock.Verify(q => q.UpdateEventQuery, Times.Never);
    }

    private void SetupMocks()
    {
        SetupTimeProvider();
        SetupQueryProvider();
        SetupConnectionFactory();
        SetupGuidGenerator();
    }
    
    private LogEventCommandHandler CreateHandler()
    {
        return new LogEventCommandHandler(ConnectionFactory, TimeProvider, GuidGenerator, QueryProvider);
    }
    
    private LogEventCommand CreateCommand()
    {
        var mu = CreateMuBot();
        var psi = CreatePsiBot();
        return new LogEventCommand(Act.Trigger(Event.Trigger(EventType.FoundPsi, mu, psi), ActType.Inspect), 10, EventStatus.Pending);
    }
    
    private LogEventCommand CreateEmptyCommand()
    {
        var mu = CreateMuBot();
        var psi = CreatePsiBot();
        return new LogEventCommand(Act.Empty(EmptyBot.Instance, EmptyBot.Instance), 0, EventStatus.Pending);
    }
}