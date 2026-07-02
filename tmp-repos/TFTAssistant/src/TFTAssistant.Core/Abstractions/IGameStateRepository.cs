using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Abstractions;

public interface IGameStateRepository
{
    Task SaveGameStateAsync(GameState gameState, CancellationToken cancellationToken = default);
    Task<GameState?> GetLatestGameStateAsync(CancellationToken cancellationToken = default);
    Task<GameState?> GetGameStateByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GameState>> GetGameStatesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task DeleteGameStatesOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
