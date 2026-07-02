using System.Collections.Immutable;

namespace TFTAssistant.Core.Models.Analytics;

public class MatchRecord : IEquatable<MatchRecord>
{
    public string Id { get; init; } = string.Empty;
    public string GameId { get; init; } = string.Empty;
    public string PlayerPuuid { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int Placement { get; init; }
    public int Level { get; init; }
    public int GoldSpent { get; init; }
    public int GoldRemaining { get; init; }
    public int DamageDealt { get; init; }
    public int DamageTaken { get; init; }
    public int RoundsPlayed { get; init; }
    public string SetVersion { get; init; } = string.Empty;
    public string QueueType { get; init; } = string.Empty;
    public bool IsComplete { get; init; }
    public ImmutableList<string> Champions { get; init; } = ImmutableList<string>.Empty;
    public ImmutableList<string> Traits { get; init; } = ImmutableList<string>.Empty;
    public ImmutableDictionary<string, int> ChampionStars { get; init; } = ImmutableDictionary<string, int>.Empty;
    public string CompName { get; init; } = string.Empty;
    public string CompId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public bool Equals(MatchRecord? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               GameId == other.GameId &&
               PlayerPuuid == other.PlayerPuuid &&
               StartTime == other.StartTime &&
               EndTime == other.EndTime &&
               Placement == other.Placement &&
               Level == other.Level &&
               GoldSpent == other.GoldSpent &&
               GoldRemaining == other.GoldRemaining &&
               DamageDealt == other.DamageDealt &&
               DamageTaken == other.DamageTaken &&
               RoundsPlayed == other.RoundsPlayed &&
               SetVersion == other.SetVersion &&
               QueueType == other.QueueType &&
               IsComplete == other.IsComplete &&
               Champions.SequenceEqual(other.Champions) &&
               Traits.SequenceEqual(other.Traits) &&
               ChampionStars.SequenceEqual(other.ChampionStars) &&
               CompName == other.CompName &&
               CompId == other.CompId &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt;
    }

    public override bool Equals(object? obj) => Equals(obj as MatchRecord);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(GameId);
        hash.Add(PlayerPuuid);
        hash.Add(StartTime);
        hash.Add(EndTime);
        hash.Add(Placement);
        hash.Add(Level);
        hash.Add(GoldSpent);
        hash.Add(GoldRemaining);
        hash.Add(DamageDealt);
        hash.Add(DamageTaken);
        hash.Add(RoundsPlayed);
        hash.Add(SetVersion);
        hash.Add(QueueType);
        hash.Add(IsComplete);
        foreach (var champion in Champions) hash.Add(champion);
        foreach (var trait in Traits) hash.Add(trait);
        foreach (var (champion, stars) in ChampionStars)
        {
            hash.Add(champion);
            hash.Add(stars);
        }
        hash.Add(CompName);
        hash.Add(CompId);
        hash.Add(CreatedAt);
        hash.Add(UpdatedAt);
        return hash.ToHashCode();
    }

    public static bool operator ==(MatchRecord? left, MatchRecord? right) => EqualityComparer<MatchRecord>.Default.Equals(left, right);
    public static bool operator !=(MatchRecord? left, MatchRecord? right) => !(left == right);
}
