using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface IDataVersionManager
{
    // 版本检查
    Task<bool> IsVersionValidAsync(string setVersion, string dataVersion, CancellationToken cancellationToken = default);
    Task<string?> GetLatestVersionAsync(string setVersion, CancellationToken cancellationToken = default);
    
    // 版本管理
    Task<MetaData> CreateOrUpdateVersionAsync(MetaData metaData, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetaData>> GetVersionHistoryAsync(string setVersion, int limit = 10, CancellationToken cancellationToken = default);
    
    // 一致性检查
    Task<bool> CheckDataConsistencyAsync(string setVersion, string version, CancellationToken cancellationToken = default);
    Task<ConsistencyCheckResult> PerformConsistencyCheckAsync(string setVersion, CancellationToken cancellationToken = default);
    
    // 版本清理
    Task DeleteOldVersionsAsync(string setVersion, int keepLatest = 3, CancellationToken cancellationToken = default);
}

public class ConsistencyCheckResult
{
    public bool IsConsistent { get; set; }
    public string Version { get; set; }
    public List<string> Issues { get; set; } = new();
    public int TotalChampions { get; set; }
    public int TotalLineups { get; set; }
    public int TotalEquipment { get; set; }
    public DateTime CheckedAt { get; set; }
}
