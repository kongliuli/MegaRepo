using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public class SqliteGameStateRepository : IGameStateRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteGameStateRepository>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqliteGameStateRepository(string connectionString, ILogger<SqliteGameStateRepository>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveGameStateAsync(GameState gameState, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        const string sql = @"
            INSERT INTO GameStates (Id, GameId, Timestamp, GameStateJson, CreatedAt)
            VALUES (@Id, @GameId, @Timestamp, @GameStateJson, @CreatedAt);
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@GameId", gameState.GameInfo.GameId);
        command.Parameters.AddWithValue("@Timestamp", gameState.Timestamp.ToString("O"));
        command.Parameters.AddWithValue("@GameStateJson", JsonSerializer.Serialize(gameState, _jsonOptions));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved GameState with Id: {Id}", id);
    }

    public async Task<GameState?> GetLatestGameStateAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT GameStateJson
            FROM GameStates
            ORDER BY Timestamp DESC
            LIMIT 1;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var json = reader.GetString(0);
            return JsonSerializer.Deserialize<GameState>(json, _jsonOptions);
        }

        return null;
    }

    public async Task<GameState?> GetGameStateByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT GameStateJson
            FROM GameStates
            WHERE Id = @Id;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var json = reader.GetString(0);
            return JsonSerializer.Deserialize<GameState>(json, _jsonOptions);
        }

        return null;
    }

    public async Task<IEnumerable<GameState>> GetGameStatesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT GameStateJson
            FROM GameStates
            WHERE Timestamp >= @StartDate AND Timestamp <= @EndDate
            ORDER BY Timestamp DESC;
        ";

        var gameStates = new List<GameState>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", endDate.ToString("O"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var json = reader.GetString(0);
            var gameState = JsonSerializer.Deserialize<GameState>(json, _jsonOptions);
            if (gameState != null)
            {
                gameStates.Add(gameState);
            }
        }

        return gameStates;
    }

    public async Task DeleteGameStatesOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM GameStates
            WHERE Timestamp < @CutoffDate;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@CutoffDate", cutoffDate.ToString("O"));

        var count = await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogInformation("Deleted {Count} GameStates older than {CutoffDate}", count, cutoffDate);
    }
}
