using System.Data;

namespace BotLife.Application.DataAccess;

public interface ISqlConnectionFactory
{
    IDbConnection? GetOpenConnection();

    IDbConnection CreateNewConnection();

    string? GetConnectionString();
}