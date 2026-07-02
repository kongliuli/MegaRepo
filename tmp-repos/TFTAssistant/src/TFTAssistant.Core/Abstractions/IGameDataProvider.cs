using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Abstractions;

public interface IGameDataProvider : IGameDataSource
{
    Task<GameState?> GetFullStateAsync();
    event EventHandler<GameStateDiff>? StateChanged;
}
