using System.Collections.Immutable;

namespace TFTAssistant.Core.Models.Analytics;

public class MatchDetail : IEquatable<MatchDetail>
{
    public string Id { get; init; } = string.Empty;
    public string MatchRecordId { get; init; } = string.Empty;
    public ImmutableList<BoardSnapshot> BoardSnapshots { get; init; } = ImmutableList<BoardSnapshot>.Empty;
    public ImmutableList<ShopSnapshot> ShopSnapshots { get; init; } = ImmutableList<ShopSnapshot>.Empty;
    public ImmutableList<AugmentSnapshot> AugmentSnapshots { get; init; } = ImmutableList<AugmentSnapshot>.Empty;
    public DateTime CreatedAt { get; init; }

    public bool Equals(MatchDetail? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               MatchRecordId == other.MatchRecordId &&
               BoardSnapshots.SequenceEqual(other.BoardSnapshots) &&
               ShopSnapshots.SequenceEqual(other.ShopSnapshots) &&
               AugmentSnapshots.SequenceEqual(other.AugmentSnapshots) &&
               CreatedAt == other.CreatedAt;
    }

    public override bool Equals(object? obj) => Equals(obj as MatchDetail);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(MatchRecordId);
        foreach (var snapshot in BoardSnapshots) hash.Add(snapshot);
        foreach (var snapshot in ShopSnapshots) hash.Add(snapshot);
        foreach (var snapshot in AugmentSnapshots) hash.Add(snapshot);
        hash.Add(CreatedAt);
        return hash.ToHashCode();
    }

    public static bool operator ==(MatchDetail? left, MatchDetail? right) => EqualityComparer<MatchDetail>.Default.Equals(left, right);
    public static bool operator !=(MatchDetail? left, MatchDetail? right) => !(left == right);
}

public class BoardSnapshot : IEquatable<BoardSnapshot>
{
    public string Id { get; init; } = string.Empty;
    public int Round { get; init; }
    public DateTime Timestamp { get; init; }
    public ImmutableList<UnitSnapshot> BoardUnits { get; init; } = ImmutableList<UnitSnapshot>.Empty;
    public ImmutableList<UnitSnapshot> BenchUnits { get; init; } = ImmutableList<UnitSnapshot>.Empty;
    public int Level { get; init; }
    public int Health { get; init; }
    public int Gold { get; init; }

    public bool Equals(BoardSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               Round == other.Round &&
               Timestamp == other.Timestamp &&
               BoardUnits.SequenceEqual(other.BoardUnits) &&
               BenchUnits.SequenceEqual(other.BenchUnits) &&
               Level == other.Level &&
               Health == other.Health &&
               Gold == other.Gold;
    }

    public override bool Equals(object? obj) => Equals(obj as BoardSnapshot);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Round);
        hash.Add(Timestamp);
        foreach (var unit in BoardUnits) hash.Add(unit);
        foreach (var unit in BenchUnits) hash.Add(unit);
        hash.Add(Level);
        hash.Add(Health);
        hash.Add(Gold);
        return hash.ToHashCode();
    }

    public static bool operator ==(BoardSnapshot? left, BoardSnapshot? right) => EqualityComparer<BoardSnapshot>.Default.Equals(left, right);
    public static bool operator !=(BoardSnapshot? left, BoardSnapshot? right) => !(left == right);
}

public class UnitSnapshot : IEquatable<UnitSnapshot>
{
    public string ChampionId { get; init; } = string.Empty;
    public int Stars { get; init; }
    public ImmutableList<string> Items { get; init; } = ImmutableList<string>.Empty;
    public int? PositionX { get; init; }
    public int? PositionY { get; init; }

    public bool Equals(UnitSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ChampionId == other.ChampionId &&
               Stars == other.Stars &&
               Items.SequenceEqual(other.Items) &&
               PositionX == other.PositionX &&
               PositionY == other.PositionY;
    }

    public override bool Equals(object? obj) => Equals(obj as UnitSnapshot);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ChampionId);
        hash.Add(Stars);
        foreach (var item in Items) hash.Add(item);
        hash.Add(PositionX);
        hash.Add(PositionY);
        return hash.ToHashCode();
    }

    public static bool operator ==(UnitSnapshot? left, UnitSnapshot? right) => EqualityComparer<UnitSnapshot>.Default.Equals(left, right);
    public static bool operator !=(UnitSnapshot? left, UnitSnapshot? right) => !(left == right);
}

public class ShopSnapshot : IEquatable<ShopSnapshot>
{
    public string Id { get; init; } = string.Empty;
    public int Round { get; init; }
    public DateTime Timestamp { get; init; }
    public ImmutableList<string> ShopChampions { get; init; } = ImmutableList<string>.Empty;
    public int Gold { get; init; }
    public int RefreshCount { get; init; }

    public bool Equals(ShopSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               Round == other.Round &&
               Timestamp == other.Timestamp &&
               ShopChampions.SequenceEqual(other.ShopChampions) &&
               Gold == other.Gold &&
               RefreshCount == other.RefreshCount;
    }

    public override bool Equals(object? obj) => Equals(obj as ShopSnapshot);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Round);
        hash.Add(Timestamp);
        foreach (var champion in ShopChampions) hash.Add(champion);
        hash.Add(Gold);
        hash.Add(RefreshCount);
        return hash.ToHashCode();
    }

    public static bool operator ==(ShopSnapshot? left, ShopSnapshot? right) => EqualityComparer<ShopSnapshot>.Default.Equals(left, right);
    public static bool operator !=(ShopSnapshot? left, ShopSnapshot? right) => !(left == right);
}

public class AugmentSnapshot : IEquatable<AugmentSnapshot>
{
    public string Id { get; init; } = string.Empty;
    public int Stage { get; init; }
    public DateTime Timestamp { get; init; }
    public ImmutableList<string> OfferedAugments { get; init; } = ImmutableList<string>.Empty;
    public string? SelectedAugment { get; init; }

    public bool Equals(AugmentSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               Stage == other.Stage &&
               Timestamp == other.Timestamp &&
               OfferedAugments.SequenceEqual(other.OfferedAugments) &&
               SelectedAugment == other.SelectedAugment;
    }

    public override bool Equals(object? obj) => Equals(obj as AugmentSnapshot);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Stage);
        hash.Add(Timestamp);
        foreach (var augment in OfferedAugments) hash.Add(augment);
        hash.Add(SelectedAugment);
        return hash.ToHashCode();
    }

    public static bool operator ==(AugmentSnapshot? left, AugmentSnapshot? right) => EqualityComparer<AugmentSnapshot>.Default.Equals(left, right);
    public static bool operator !=(AugmentSnapshot? left, AugmentSnapshot? right) => !(left == right);
}
