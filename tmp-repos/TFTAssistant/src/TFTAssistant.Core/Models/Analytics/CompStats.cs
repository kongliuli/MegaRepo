using System.Collections.Immutable;

namespace TFTAssistant.Core.Models.Analytics;

public class CompStats : IEquatable<CompStats>
{
    public string Id { get; init; } = string.Empty;
    public string CompId { get; init; } = string.Empty;
    public string CompName { get; init; } = string.Empty;
    public string PlayerPuuid { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int TotalGames { get; init; }
    public int Wins { get; init; }
    public int Top4 { get; init; }
    public double WinRate { get; init; }
    public double Top4Rate { get; init; }
    public double AveragePlacement { get; init; }
    public double AverageLevel { get; init; }
    public double AverageRoundsPlayed { get; init; }
    public ImmutableList<string> CoreChampions { get; init; } = ImmutableList<string>.Empty;
    public ImmutableDictionary<string, int> ChampionPickCounts { get; init; } = ImmutableDictionary<string, int>.Empty;
    public string SetVersion { get; init; } = string.Empty;
    public string QueueType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool IsHistoricalData { get; init; } = true;
    public string DataSourceNote { get; init; } = "历史统计数据，仅供参考";

    public bool Equals(CompStats? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               CompId == other.CompId &&
               CompName == other.CompName &&
               PlayerPuuid == other.PlayerPuuid &&
               PeriodStart == other.PeriodStart &&
               PeriodEnd == other.PeriodEnd &&
               TotalGames == other.TotalGames &&
               Wins == other.Wins &&
               Top4 == other.Top4 &&
               WinRate == other.WinRate &&
               Top4Rate == other.Top4Rate &&
               AveragePlacement == other.AveragePlacement &&
               AverageLevel == other.AverageLevel &&
               AverageRoundsPlayed == other.AverageRoundsPlayed &&
               CoreChampions.SequenceEqual(other.CoreChampions) &&
               ChampionPickCounts.SequenceEqual(other.ChampionPickCounts) &&
               SetVersion == other.SetVersion &&
               QueueType == other.QueueType &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt &&
               IsHistoricalData == other.IsHistoricalData &&
               DataSourceNote == other.DataSourceNote;
    }

    public override bool Equals(object? obj) => Equals(obj as CompStats);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(CompId);
        hash.Add(CompName);
        hash.Add(PlayerPuuid);
        hash.Add(PeriodStart);
        hash.Add(PeriodEnd);
        hash.Add(TotalGames);
        hash.Add(Wins);
        hash.Add(Top4);
        hash.Add(WinRate);
        hash.Add(Top4Rate);
        hash.Add(AveragePlacement);
        hash.Add(AverageLevel);
        hash.Add(AverageRoundsPlayed);
        foreach (var champion in CoreChampions) hash.Add(champion);
        foreach (var (champion, count) in ChampionPickCounts)
        {
            hash.Add(champion);
            hash.Add(count);
        }
        hash.Add(SetVersion);
        hash.Add(QueueType);
        hash.Add(CreatedAt);
        hash.Add(UpdatedAt);
        hash.Add(IsHistoricalData);
        hash.Add(DataSourceNote);
        return hash.ToHashCode();
    }

    public static bool operator ==(CompStats? left, CompStats? right) => EqualityComparer<CompStats>.Default.Equals(left, right);
    public static bool operator !=(CompStats? left, CompStats? right) => !(left == right);
}
