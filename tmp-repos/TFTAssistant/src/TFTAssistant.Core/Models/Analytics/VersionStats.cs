namespace TFTAssistant.Core.Models.Analytics;

public class VersionStats : IEquatable<VersionStats>
{
    public string Id { get; init; } = string.Empty;
    public string SetVersion { get; init; } = string.Empty;
    public string PlayerPuuid { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int TotalMatches { get; init; }
    public int Wins { get; init; }
    public int Top4 { get; init; }
    public int Top8 { get; init; }
    public double WinRate { get; init; }
    public double Top4Rate { get; init; }
    public double AveragePlacement { get; init; }
    public double AverageLevel { get; init; }
    public double AverageGoldSpent { get; init; }
    public double AverageRoundsPlayed { get; init; }
    public string QueueType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool IsHistoricalData { get; init; } = true;
    public string DataSourceNote { get; init; } = "历史统计数据，仅供参考";

    public bool Equals(VersionStats? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id &&
               SetVersion == other.SetVersion &&
               PlayerPuuid == other.PlayerPuuid &&
               PeriodStart == other.PeriodStart &&
               PeriodEnd == other.PeriodEnd &&
               TotalMatches == other.TotalMatches &&
               Wins == other.Wins &&
               Top4 == other.Top4 &&
               Top8 == other.Top8 &&
               WinRate == other.WinRate &&
               Top4Rate == other.Top4Rate &&
               AveragePlacement == other.AveragePlacement &&
               AverageLevel == other.AverageLevel &&
               AverageGoldSpent == other.AverageGoldSpent &&
               AverageRoundsPlayed == other.AverageRoundsPlayed &&
               QueueType == other.QueueType &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt &&
               IsHistoricalData == other.IsHistoricalData &&
               DataSourceNote == other.DataSourceNote;
    }

    public override bool Equals(object? obj) => Equals(obj as VersionStats);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(SetVersion);
        hash.Add(PlayerPuuid);
        hash.Add(PeriodStart);
        hash.Add(PeriodEnd);
        hash.Add(TotalMatches);
        hash.Add(Wins);
        hash.Add(Top4);
        hash.Add(Top8);
        hash.Add(WinRate);
        hash.Add(Top4Rate);
        hash.Add(AveragePlacement);
        hash.Add(AverageLevel);
        hash.Add(AverageGoldSpent);
        hash.Add(AverageRoundsPlayed);
        hash.Add(QueueType);
        hash.Add(CreatedAt);
        hash.Add(UpdatedAt);
        hash.Add(IsHistoricalData);
        hash.Add(DataSourceNote);
        return hash.ToHashCode();
    }

    public static bool operator ==(VersionStats? left, VersionStats? right) => EqualityComparer<VersionStats>.Default.Equals(left, right);
    public static bool operator !=(VersionStats? left, VersionStats? right) => !(left == right);
}
