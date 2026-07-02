using System.Collections.Immutable;

namespace TFTAssistant.Core.Models.Analytics;

public class RoundSnapshot : IEquatable<RoundSnapshot>
{
    public string Id { get; init; } = string.Empty;
    public string MatchRecordId { get; init; } = string.Empty;
    public int RoundNumber { get; init; }
    public string Stage { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public int Level { get; init; }
    public int Health { get; init; }
    public int Gold { get; init; }
    public int Experience { get; init; }
    public ImmutableList<UnitSnapshot> BoardUnits { get; init; } = ImmutableList<UnitSnapshot>.Empty;
    public ImmutableList<UnitSnapshot> BenchUnits { get; init; } = ImmutableList<UnitSnapshot>.Empty;
    public ImmutableList<string> ActiveTraits { get; init; } = ImmutableList<string>.Empty;
    public ImmutableDictionary<string, int> TraitCounts { get; init; } = ImmutableDictionary<string, int>.Empty;
    public bool IsPvpRound { get; init; }
    public string? OpponentPlayerId { get; init; }
    public int? OpponentHealth { get; init; }
    public int DamageDealtThisRound { get; init; }
    public int DamageTakenThisRound { get; init; }
    public int GoldSpentThisRound { get; init; }
    public int RefreshCountThisRound { get; init; }
    public DateTime CreatedAt { get; init; }

    public bool Equals(RoundSnapshot? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               MatchRecordId == other.MatchRecordId &&
               RoundNumber == other.RoundNumber &&
               Stage == other.Stage &&
               Timestamp == other.Timestamp &&
               Level == other.Level &&
               Health == other.Health &&
               Gold == other.Gold &&
               Experience == other.Experience &&
               BoardUnits.SequenceEqual(other.BoardUnits) &&
               BenchUnits.SequenceEqual(other.BenchUnits) &&
               ActiveTraits.SequenceEqual(other.ActiveTraits) &&
               TraitCounts.SequenceEqual(other.TraitCounts) &&
               IsPvpRound == other.IsPvpRound &&
               OpponentPlayerId == other.OpponentPlayerId &&
               OpponentHealth == other.OpponentHealth &&
               DamageDealtThisRound == other.DamageDealtThisRound &&
               DamageTakenThisRound == other.DamageTakenThisRound &&
               GoldSpentThisRound == other.GoldSpentThisRound &&
               RefreshCountThisRound == other.RefreshCountThisRound &&
               CreatedAt == other.CreatedAt;
    }

    public override bool Equals(object? obj) => Equals(obj as RoundSnapshot);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(MatchRecordId);
        hash.Add(RoundNumber);
        hash.Add(Stage);
        hash.Add(Timestamp);
        hash.Add(Level);
        hash.Add(Health);
        hash.Add(Gold);
        hash.Add(Experience);
        foreach (var unit in BoardUnits) hash.Add(unit);
        foreach (var unit in BenchUnits) hash.Add(unit);
        foreach (var trait in ActiveTraits) hash.Add(trait);
        foreach (var (trait, count) in TraitCounts)
        {
            hash.Add(trait);
            hash.Add(count);
        }
        hash.Add(IsPvpRound);
        hash.Add(OpponentPlayerId);
        hash.Add(OpponentHealth);
        hash.Add(DamageDealtThisRound);
        hash.Add(DamageTakenThisRound);
        hash.Add(GoldSpentThisRound);
        hash.Add(RefreshCountThisRound);
        hash.Add(CreatedAt);
        return hash.ToHashCode();
    }

    public static bool operator ==(RoundSnapshot? left, RoundSnapshot? right) => EqualityComparer<RoundSnapshot>.Default.Equals(left, right);
    public static bool operator !=(RoundSnapshot? left, RoundSnapshot? right) => !(left == right);
}
