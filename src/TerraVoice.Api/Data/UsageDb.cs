using Microsoft.Data.Sqlite;

namespace TerraVoice.Api.Data;

public sealed class UsageDb
{
    private readonly string _connectionString;

    public UsageDb(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("UsageDb")
                            ?? "Data Source=Data/usage.db";
    }

    private void EnsureSqliteDirectoryExists()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource))
            return;

        var directory = Path.GetDirectoryName(dataSource);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    public async Task InitializeAsync()
    {
        EnsureSqliteDirectoryExists(); // ensure the database directory exists

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS usage_monthly (
                                  provider TEXT NOT NULL,
                                  month TEXT NOT NULL,
                                  request_count INTEGER NOT NULL DEFAULT 0,
                                  char_count INTEGER NOT NULL DEFAULT 0,
                                  updated_at TEXT NOT NULL,
                                  PRIMARY KEY (provider, month)
                                  );
                              """;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Checks whether the SQLite database is reachable and able to execute a simple query.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the database connection is successfully opened and the test query completes; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> IsReadyAsync()
    {
        try
        {
            await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task RecordAsync(string provider, string month, int charCount)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                                  INSERT INTO usage_monthly (provider, month, request_count, char_count, updated_at)
                                  VALUES($provider, $month, 1, $charCount, $updateAt)
                                  ON CONFLICT(provider, month) DO UPDATE SET
                                     request_count = request_count + 1,
                                     char_count = char_count + $charCount,
                                     updated_at = $updateAt;
                              """;

        command.Parameters.AddWithValue("$provider", provider);
        command.Parameters.AddWithValue("$month", month);
        command.Parameters.AddWithValue("$charCount", charCount);
        command.Parameters.AddWithValue("$updateAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<(int RequestCount, int CharCount)> GetMonthlyAsync(string provider, string month)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT request_count, char_count
                              FROM usage_monthly
                              WHERE provider = $provider AND month = $month;
                              """;

        command.Parameters.AddWithValue("$provider", provider);
        command.Parameters.AddWithValue("$month", month);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return (0, 0);

        return (reader.GetInt32(0), reader.GetInt32(1));
    }
}