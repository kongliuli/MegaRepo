using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class InMemoryBigDataCache : IBigDataCache
{
    private class CacheItem<T>
    {
        public T Value { get; set; }
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }
        public long SizeEstimate { get; set; } // 估计内存大小
    }

    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly int _maxCacheSize;
    private readonly TimeSpan _cacheExpiration;
    private readonly long _maxMemorySize; // 最大内存使用量（字节）
    private readonly ILogger<InMemoryBigDataCache>? _logger;
    private long _currentMemorySize = 0; // 当前内存使用量

    public InMemoryBigDataCache(int maxCacheSize = 200, TimeSpan? cacheExpiration = null, long maxMemorySize = 100 * 1024 * 1024, ILogger<InMemoryBigDataCache>? logger = null)
    {
        _maxCacheSize = maxCacheSize;
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromHours(12); // 缩短过期时间
        _maxMemorySize = maxMemorySize; // 默认100MB
        _logger = logger;
    }

    public async Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var key = $"metadata:{setVersion}";
        return await GetAsync<MetaData>(key, cancellationToken);
    }

    public async Task SetMetaDataAsync(string setVersion, MetaData metaData, CancellationToken cancellationToken = default)
    {
        var key = $"metadata:{setVersion}";
        await SetAsync(key, metaData, cancellationToken);
    }

    public async Task<IEnumerable<LineupData>?> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var key = $"lineups:{setVersion}";
        return await GetAsync<IEnumerable<LineupData>>(key, cancellationToken);
    }

    public async Task SetLineupDataAsync(string setVersion, IEnumerable<LineupData> lineups, CancellationToken cancellationToken = default)
    {
        var key = $"lineups:{setVersion}";
        await SetAsync(key, lineups, cancellationToken);
    }

    public async Task<IEnumerable<EquipmentData>?> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default)
    {
        var key = $"equipment:{setVersion}:{isComponent}";
        return await GetAsync<IEnumerable<EquipmentData>>(key, cancellationToken);
    }

    public async Task SetEquipmentDataAsync(string setVersion, IEnumerable<EquipmentData> equipmentList, CancellationToken cancellationToken = default)
    {
        var key = $"equipment:{setVersion}:null";
        await SetAsync(key, equipmentList, cancellationToken);
        
        // 按组件类型分开缓存
        var components = equipmentList.Where(e => e.IsComponent).ToList();
        var nonComponents = equipmentList.Where(e => !e.IsComponent).ToList();
        
        if (components.Any())
        {
            var componentKey = $"equipment:{setVersion}:True";
            await SetAsync(componentKey, components, cancellationToken);
        }
        
        if (nonComponents.Any())
        {
            var nonComponentKey = $"equipment:{setVersion}:False";
            await SetAsync(nonComponentKey, nonComponents, cancellationToken);
        }
    }

    public async Task<IEnumerable<ChampionData>?> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default)
    {
        var key = $"champions:{setVersion}:{cost}";
        return await GetAsync<IEnumerable<ChampionData>>(key, cancellationToken);
    }

    public async Task SetChampionDataAsync(string setVersion, IEnumerable<ChampionData> champions, CancellationToken cancellationToken = default)
    {
        var key = $"champions:{setVersion}:null";
        await SetAsync(key, champions, cancellationToken);
        
        // 按费用分开缓存
        var costGroups = champions.GroupBy(c => c.Cost);
        foreach (var group in costGroups)
        {
            var costKey = $"champions:{setVersion}:{group.Key}";
            await SetAsync(costKey, group.ToList(), cancellationToken);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        _currentMemorySize = 0;
        _logger?.LogInformation("Cache cleared, Memory freed: {Size} bytes", _currentMemorySize);
        await Task.CompletedTask;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            var cacheItem = item as dynamic;
            if (cacheItem != null)
            {
                _currentMemorySize -= cacheItem.SizeEstimate;
            }
        }
        _cache.TryRemove(key, out _);
        _logger?.LogDebug("Removed item from cache: {Key}, Current Memory: {CurrentMemory} bytes", key, _currentMemorySize);
        await Task.CompletedTask;
    }

    public async Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default)
    {
        return _cache.Count;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (_cache.TryGetValue(key, out var cachedItem))
        {
            var item = cachedItem as CacheItem<T>;
            if (item != null && IsValid(item))
            {
                // 更新访问时间和次数
                item.LastAccessed = DateTime.UtcNow;
                item.AccessCount++;
                _logger?.LogDebug("Cache hit for key: {Key}, AccessCount: {AccessCount}", key, item.AccessCount);
                return item.Value;
            }
            else
            {
                // 移除过期项
                _cache.TryRemove(key, out _);
                _logger?.LogDebug("Cache expired for key: {Key}", key);
            }
        }
        
        _logger?.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // 估计对象大小（简化计算）
        long sizeEstimate = EstimateObjectSize(value);
        
        // 检查缓存大小和内存使用
        while (_cache.Count >= _maxCacheSize || _currentMemorySize + sizeEstimate > _maxMemorySize)
        {
            EvictItems();
        }
        
        // 移除旧值（如果存在）
        if (_cache.TryGetValue(key, out var oldItem))
        {
            var oldCacheItem = oldItem as dynamic;
            if (oldCacheItem != null)
            {
                _currentMemorySize -= oldCacheItem.SizeEstimate;
            }
        }
        
        var cacheItem = new CacheItem<T>
        {
            Value = value,
            LastAccessed = DateTime.UtcNow,
            AccessCount = 1,
            SizeEstimate = sizeEstimate
        };
        
        _cache[key] = cacheItem;
        _currentMemorySize += sizeEstimate;
        _logger?.LogDebug("Cache set for key: {Key}, Size: {Size} bytes, Current Memory: {CurrentMemory} bytes", 
            key, sizeEstimate, _currentMemorySize);
        await Task.CompletedTask;
    }
    
    private long EstimateObjectSize(object obj)
    {
        if (obj == null)
            return 0;
        
        // 简化的对象大小估计
        long size = 0;
        
        // 基本类型大小
        if (obj is string str)
            return str.Length * 2; // 假设每个字符2字节
        if (obj is int || obj is float) return 4;
        if (obj is long || obj is double) return 8;
        if (obj is bool) return 1;
        
        // 集合类型
        if (obj is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                size += EstimateObjectSize(item);
            }
        }
        else
        {
            // 复杂对象，估计一个默认大小
            size = 100; // 假设每个复杂对象100字节
        }
        
        return size;
    }

    private bool IsValid<T>(CacheItem<T> item)
    {
        return DateTime.UtcNow - item.LastAccessed < _cacheExpiration;
    }

    private void EvictItems()
    {
        // 按访问频率、时间和大小排序，优先移除访问频率低、占用内存大的项
        var itemsToRemove = _cache
            .OrderBy(kv => {
                var item = kv.Value as dynamic;
                return (item?.AccessCount ?? 0, item?.LastAccessed ?? DateTime.MinValue, -(item?.SizeEstimate ?? 0));
            })
            .Take(Math.Max(1, _cache.Count / 4)) // 每次移除1/4的项
            .Select(kv => kv.Key)
            .ToList();
        
        long evictedSize = 0;
        foreach (var key in itemsToRemove)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                var cacheItem = item as dynamic;
                if (cacheItem != null)
                {
                    evictedSize += cacheItem.SizeEstimate;
                }
            }
            _cache.TryRemove(key, out _);
            _logger?.LogDebug("Evicted item from cache: {Key}", key);
        }
        
        _currentMemorySize -= evictedSize;
        _logger?.LogDebug("Evicted {Count} items, freed {Size} bytes, Current Memory: {CurrentMemory} bytes", 
            itemsToRemove.Count, evictedSize, _currentMemorySize);
    }
}