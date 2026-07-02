namespace TFTAssistant.Core.Models.Game;

public sealed class BenchUnit : IEquatable<BenchUnit>
{
    public required string ChampionName { get; init; }
    public required int StarLevel { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
    public required int BenchSlot { get; init; }

    public bool Equals(BenchUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName
            && StarLevel == other.StarLevel
            && BenchSlot == other.BenchSlot;
    }

    public override bool Equals(object? obj) => Equals(obj as BenchUnit);

    public override int GetHashCode() => HashCode.Combine(ChampionName, BenchSlot);
}
