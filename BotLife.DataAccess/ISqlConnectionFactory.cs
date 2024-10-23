using System.Data;

namespace BotLife.DataAccess;

public interface ISqlConnectionFactory
{
    IDbConnection? GetOpenConnection();

    IDbConnection CreateNewConnection();

    string? GetConnectionString();
}