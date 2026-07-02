namespace TFTAssistant.Core.Models.Game;

public sealed class PlayerSummary
{
    public required string SummonerName { get; init; }
    public required double Health { get; init; }
    public required int Placement { get; init; }
    public required int TotalGold { get; init; }
    public required int Level { get; init; }
}
