using System.Data;
using BotLife.Application.Bot;
using BotLife.Application.DataAccess;
using BotLife.Application.DataAccess.Models;
using Dapper;
using Moq;
using Moq.Dapper;

namespace BotLife.Application.Tests;

public class BotLifeTestDbBase : BotLifeTestBase
{
    protected ISqlConnectionFactory ConnectionFactory => ConnectionFactoryMock.Object;
    protected IDbConnection DbConnection => DbConnectionMock.Object;
    protected Bot.LogEvent.IQueryProvider QueryProvider => QueryProviderMock.Object;
    protected Mock<ISqlConnectionFactory> ConnectionFactoryMock { get; } = new();
    protected Mock<IDbConnection> DbConnectionMock { get; } = new();
    protected Mock<Bot.LogEvent.IQueryProvider> QueryProviderMock { get; } = new();

    protected BotLifeTestDbBase()
    {
    }
    
    // Set up query provider
    protected virtual void SetupQueryProvider()
    {
        QueryProviderMock.Setup(m => m.GetEventQuery).Returns("SELECT * FROM [Event]");
        QueryProviderMock.Setup(m => m.UpdateEventQuery).Returns("UPDATE [Event] SET Status = ''");
        QueryProviderMock.Setup(m => m.InsertEventQuery).Returns("INSERT INTO [Event] (Id) VALUES ('')");
    }
    
    // Set up connection factory
    protected virtual void SetupConnectionFactory()
    {
        ConnectionFactoryMock.Setup(m => m.GetOpenConnection()).Returns(DbConnection);
    }
    
    // Set up dapper
    protected virtual void SetupDapper(List<EventModel>? events = null)
    {
        DbConnectionMock.SetupDapperAsync(c => 
                c.QueryAsync<EventModel>(
                    It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(events ?? []);
    }
    
    // Set up event model list
    protected virtual List<EventModel> GetEventModelList()
    {
        var eventModel = new EventModel(
            Guid.NewGuid(),
            Guid.NewGuid(), 
            BotType.Mu.ToString(),
            EventType.None.ToString(),
            ActType.WalkAround.ToString(),
            10M,
            null,
            EventStatus.Pending.ToString(),
            DateTime.Now,
            null
        );
        return [eventModel];
    }
}
