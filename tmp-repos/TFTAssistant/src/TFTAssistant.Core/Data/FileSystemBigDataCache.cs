using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class FileSystemBigDataCache : IBigDataCache
{
    private readonly string _cacheDirectory;
    private readonly int _maxCacheSize;
    private readonly TimeSpan _cacheExpiration;
    private readonly ILogger<FileSystemBigDataCache>? _logger;

    public FileSystemBigDataCache(
        string cacheDirectory,
        int maxCacheSize = 100,
        TimeSpan? cacheExpiration = null,
        ILogger<FileSystemBigDataCache>? logger = null)
    {
        _cacheDirectory = cacheDirectory;
        _maxCacheSize = maxCacheSize;
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromHours(24);
        _logger = logger;

        // 确保缓存目录存在
        Directory.CreateDirectory(_cacheDirectory);
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
        try
        {
            var files = Directory.GetFiles(_cacheDirectory, "*.json.gz", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            _logger?.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error clearing cache");
        }
        await Task.CompletedTask;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger?.LogDebug("Removed item from cache: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error removing item from cache: {Key}", key);
        }
        await Task.CompletedTask;
    }

    public async Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var files = Directory.GetFiles(_cacheDirectory, "*.json.gz", SearchOption.AllDirectories);
            return files.Length;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting cache size");
            return 0;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var filePath = GetCacheFilePath(key);
            if (!File.Exists(filePath))
            {
                _logger?.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            // 检查文件是否过期
            var fileInfo = new FileInfo(filePath);
            if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > _cacheExpiration)
            {
                File.Delete(filePath);
                _logger?.LogDebug("Cache expired for key: {Key}", key);
                return default;
            }

            // 读取并反序列化缓存数据
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            var json = await reader.ReadToEndAsync(cancellationToken);
            var value = JsonSerializer.Deserialize<T>(json);
            
            _logger?.LogDebug("Cache hit for key: {Key}", key);
            return value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading from cache: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            // 检查缓存大小
            var files = Directory.GetFiles(_cacheDirectory, "*.json.gz", SearchOption.AllDirectories);
            if (files.Length >= _maxCacheSize)
            {
                EvictItems();
            }

            // 序列化并压缩数据
            var json = JsonSerializer.Serialize(value);
            var filePath = GetCacheFilePath(key);
            
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // 写入缓存文件
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
            using var writer = new StreamWriter(gzipStream);
            await writer.WriteAsync(json, cancellationToken);
            
            _logger?.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error writing to cache: {Key}", key);
        }
    }

    private string GetCacheFilePath(string key)
    {
        // 生成安全的文件名
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{safeKey}.json.gz");
    }

    private void EvictItems()
    {
        try
        {
            var files = Directory.GetFiles(_cacheDirectory, "*.json.gz", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .OrderBy(fi => fi.LastWriteTimeUtc)
                .Take(_maxCacheSize / 2)
                .ToList();

            foreach (var file in files)
            {
                file.Delete();
                _logger?.LogDebug("Evicted item from cache: {File}", file.Name);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error evicting items from cache");
        }
    }