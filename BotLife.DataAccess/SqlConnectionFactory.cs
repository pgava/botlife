using System.Data;
using System.Data.SqlClient;

namespace BotLife.DataAccess;

public class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
{
    private readonly string? _connectionString;
    private IDbConnection? _connection;

    public SqlConnectionFactory(string? connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection? GetOpenConnection()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_connectionString);
        
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }

        return _connection;
    }

    public IDbConnection CreateNewConnection()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_connectionString);

        var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection;
    }

    public string? GetConnectionString()
    {
        return _connectionString;
    }

    public void Dispose()
    {
        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Dispose();
        }
    }
}