namespace TFTAssistant.Core.Models.Game;

public sealed class ShopUnit : IEquatable<ShopUnit>
{
    public required string ChampionName { get; init; }
    public required int Cost { get; init; }
    public required int ShopSlot { get; init; }

    public bool Equals(ShopUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName && ShopSlot == other.ShopSlot;
    }

    public override bool Equals(object? obj) => Equals(obj as ShopUnit);

    public override int GetHashCode() => HashCode.Combine(ChampionName, ShopSlot);
}
