using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface IBigDataCache
{
    // 元数据缓存操作
    Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default);
    Task SetMetaDataAsync(string setVersion, MetaData metaData, CancellationToken cancellationToken = default);
    
    // 阵容数据缓存操作
    Task<IEnumerable<LineupData>?> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default);
    Task SetLineupDataAsync(string setVersion, IEnumerable<LineupData> lineups, CancellationToken cancellationToken = default);
    
    // 装备数据缓存操作
    Task<IEnumerable<EquipmentData>?> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default);
    Task SetEquipmentDataAsync(string setVersion, IEnumerable<EquipmentData> equipmentList, CancellationToken cancellationToken = default);
    
    // 英雄数据缓存操作
    Task<IEnumerable<ChampionData>?> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default);
    Task SetChampionDataAsync(string setVersion, IEnumerable<ChampionData> champions, CancellationToken cancellationToken = default);
    
    // 通用缓存操作
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;
    
    // 缓存管理
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default);
}
