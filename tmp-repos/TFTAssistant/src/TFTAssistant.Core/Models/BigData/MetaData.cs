namespace TFTAssistant.Core.Models.BigData;

public sealed class MetaData
{
    public string Id { get; init; } = "";
    public string Version { get; init; } = "";
    public string SetVersion { get; init; } = "";
    public DateTime LastUpdated { get; init; }
    public string DataSource { get; init; } = "";
    public int TotalMatches { get; init; }
    public int TotalPlayers { get; init; }
}
