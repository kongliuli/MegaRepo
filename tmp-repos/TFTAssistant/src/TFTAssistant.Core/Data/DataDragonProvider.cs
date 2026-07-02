using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Data;

public sealed class DataDragonProvider : IStaticDataProvider
{
    private const string DRAGON_BASE = "https://ddragon.leagueoflegends.com/cdn";
    private const string VERSIONS_URL = "https://ddragon.leagueoflegends.com/api/versions.json";

    private readonly HttpClient _http;
    private readonly ILogger<DataDragonProvider> _logger;
    private readonly string _baseDataDir;
    private readonly JsonSerializerOptions _jsonOptions;

    private string? _cachedLatestVersion;
    private IReadOnlyList<string>? _cachedAvailableVersions;
    private readonly Dictionary<string, Dictionary<string, object>> _memoryCache;
    private readonly object _cacheLock = new();

    public DataDragonProvider(
        HttpClient http,
        ILogger<DataDragonProvider> logger,
        string baseDataDir = "data")
    {
        _http = http;
        _logger = logger;
        _baseDataDir = baseDataDir;
        _memoryCache = new Dictionary<string, Dictionary<string, object>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMs = 1000;

    public async Task<IReadOnlyList<string>> GetAvailableVersionsAsync()
    {
        if (_cachedAvailableVersions != null)
            return _cachedAvailableVersions;

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var versions = await _http.GetFromJsonAsync<List<string>>(VERSIONS_URL);
                _cachedAvailableVersions = versions ?? new List<string>();
                _logger.LogInformation("成功获取 Data Dragon 可用版本列表");
                return _cachedAvailableVersions;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "获取 Data Dragon 版本列表失败 (尝试 {Attempt}/{Max})");
                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 Data Dragon 版本列表时发生未知错误");
                break;
            }
        }
        
        _logger.LogInformation("使用缓存的版本列表");
        return await GetCachedVersionsAsync();
    }

    public async Task<string> GetLatestVersionAsync()
    {
        if (_cachedLatestVersion != null)
            return _cachedLatestVersion;

        var versions = await GetAvailableVersionsAsync();
        _cachedLatestVersion = versions.Count > 0 ? versions[0] : "15.1.1";
        return _cachedLatestVersion;
    }

    public async Task<IReadOnlyList<Champion>> GetChampionsAsync(string set, string? version = null)
    {
        var actualVersion = await ResolveVersionAsync(version);
        var cacheKey = $"champions_{set}";

        if (TryGetFromMemoryCache<List<Champion>>(actualVersion, cacheKey, out var cached))
            return cached;

        var localPath = GetLocalFilePath(actualVersion, set, "champions.json");
        if (File.Exists(localPath))
        {
            try
            {
                var champions = await LoadFromLocalFileAsync<List<Champion>>(localPath);
                SetMemoryCache(actualVersion, cacheKey, champions);
                _logger.LogDebug("从本地缓存加载英雄数据: {Set} - {Version}", set, actualVersion);
                return champions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载本地英雄数据失败: {Path}", localPath);
            }
        }

        var url = $"{DRAGON_BASE}/{actualVersion}/data/en_US/tft-champion.json";
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var response = await _http.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);
                var champions = ParseChampions(data);

                try
                {
                    await SaveToLocalFileAsync(localPath, champions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "保存英雄数据到本地失败");
                }
                
                SetMemoryCache(actualVersion, cacheKey, champions);
                _logger.LogInformation("成功获取英雄数据: {Set} - {Version}", set, actualVersion);
                return champions;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "获取英雄数据失败 (尝试 {Attempt}/{Max}): {Version}", attempt, MaxRetryAttempts, actualVersion);
                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析英雄数据失败: {Version}", actualVersion);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取英雄数据时发生未知错误: {Version}", actualVersion);
                break;
            }
        }
        
        _logger.LogWarning("无法获取英雄数据，返回空列表: {Set} - {Version}", set, actualVersion);
        return new List<Champion>();
    }

    public async Task<IReadOnlyList<Item>> GetItemsAsync(string set, string? version = null)
    {
        var actualVersion = await ResolveVersionAsync(version);
        var cacheKey = $"items_{set}";

        if (TryGetFromMemoryCache<List<Item>>(actualVersion, cacheKey, out var cached))
            return cached;

        var localPath = GetLocalFilePath(actualVersion, set, "items.json");
        if (File.Exists(localPath))
        {
            try
            {
                var items = await LoadFromLocalFileAsync<List<Item>>(localPath);
                SetMemoryCache(actualVersion, cacheKey, items);
                _logger.LogDebug("从本地缓存加载装备数据: {Set} - {Version}", set, actualVersion);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载本地装备数据失败: {Path}", localPath);
            }
        }

        var url = $"{DRAGON_BASE}/{actualVersion}/data/en_US/tft-item.json";
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var response = await _http.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);
                var items = ParseItems(data);

                try
                {
                    await SaveToLocalFileAsync(localPath, items);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "保存装备数据到本地失败");
                }
                
                SetMemoryCache(actualVersion, cacheKey, items);
                _logger.LogInformation("成功获取装备数据: {Set} - {Version}", set, actualVersion);
                return items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "获取装备数据失败 (尝试 {Attempt}/{Max}): {Version}", attempt, MaxRetryAttempts, actualVersion);
                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析装备数据失败: {Version}", actualVersion);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取装备数据时发生未知错误: {Version}", actualVersion);
                break;
            }
        }
        
        _logger.LogWarning("无法获取装备数据，返回空列表: {Set} - {Version}", set, actualVersion);
        return new List<Item>();
    }

    public async Task<IReadOnlyList<Trait>> GetTraitsAsync(string set, string? version = null)
    {
        var actualVersion = await ResolveVersionAsync(version);
        var cacheKey = $"traits_{set}";

        if (TryGetFromMemoryCache<List<Trait>>(actualVersion, cacheKey, out var cached))
            return cached;

        var localPath = GetLocalFilePath(actualVersion, set, "traits.json");
        if (File.Exists(localPath))
        {
            try
            {
                var traits = await LoadFromLocalFileAsync<List<Trait>>(localPath);
                SetMemoryCache(actualVersion, cacheKey, traits);
                _logger.LogDebug("从本地缓存加载特质数据: {Set} - {Version}", set, actualVersion);
                return traits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载本地特质数据失败: {Path}", localPath);
            }
        }

        var url = $"{DRAGON_BASE}/{actualVersion}/data/en_US/tft-trait.json";
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var response = await _http.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);
                var traits = ParseTraits(data);

                try
                {
                    await SaveToLocalFileAsync(localPath, traits);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "保存特质数据到本地失败");
                }
                
                SetMemoryCache(actualVersion, cacheKey, traits);
                _logger.LogInformation("成功获取特质数据: {Set} - {Version}", set, actualVersion);
                return traits;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "获取特质数据失败 (尝试 {Attempt}/{Max}): {Version}", attempt, MaxRetryAttempts, actualVersion);
                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析特质数据失败: {Version}", actualVersion);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取特质数据时发生未知错误: {Version}", actualVersion);
                break;
            }
        }
        
        _logger.LogWarning("无法获取特质数据，返回空列表: {Set} - {Version}", set, actualVersion);
        return new List<Trait>();
    }

    public async Task<IReadOnlyList<AugmentData>> GetAugmentsAsync(string set, string? version = null)
    {
        var actualVersion = await ResolveVersionAsync(version);
        var cacheKey = $"augments_{set}";

        if (TryGetFromMemoryCache<List<AugmentData>>(actualVersion, cacheKey, out var cached))
            return cached;

        var localPath = GetLocalFilePath(actualVersion, set, "augments.json");
        if (File.Exists(localPath))
        {
            var augments = await LoadFromLocalFileAsync<List<AugmentData>>(localPath);
            SetMemoryCache(actualVersion, cacheKey, augments);
            return augments;
        }

        var augmentsList = new List<AugmentData>();
        await SaveToLocalFileAsync(localPath, augmentsList);
        SetMemoryCache(actualVersion, cacheKey, augmentsList);
        return augmentsList;
    }

    public async Task<IReadOnlyList<MetaComp>> GetMetaCompsAsync(string set, string? version = null)
    {
        var actualVersion = await ResolveVersionAsync(version);
        var cacheKey = $"metaComps_{set}";

        if (TryGetFromMemoryCache<List<MetaComp>>(actualVersion, cacheKey, out var cached))
            return cached;

        var localPath = GetLocalFilePath(actualVersion, set, "meta-comps.json");
        if (File.Exists(localPath))
        {
            var metaComps = await LoadFromLocalFileAsync<List<MetaComp>>(localPath);
            SetMemoryCache(actualVersion, cacheKey, metaComps);
            return metaComps;
        }

        var metaCompsList = new List<MetaComp>();
        await SaveToLocalFileAsync(localPath, metaCompsList);
        SetMemoryCache(actualVersion, cacheKey, metaCompsList);
        return metaCompsList;
    }

    public Task ClearVersionCacheAsync(string? version = null)
    {
        lock (_cacheLock)
        {
            if (version != null)
            {
                _memoryCache.Remove(version);
                var versionDir = Path.Combine(_baseDataDir, "versions", version);
                if (Directory.Exists(versionDir))
                {
                    Directory.Delete(versionDir, true);
                    _logger.LogInformation("Cleared cache for version {Version}", version);
                }
            }
            else
            {
                _memoryCache.Clear();
                _cachedLatestVersion = null;
                _cachedAvailableVersions = null;
                var versionsDir = Path.Combine(_baseDataDir, "versions");
                if (Directory.Exists(versionsDir))
                {
                    Directory.Delete(versionsDir, true);
                    _logger.LogInformation("Cleared all version caches");
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetCachedVersionsAsync()
    {
        var versionsDir = Path.Combine(_baseDataDir, "versions");
        if (!Directory.Exists(versionsDir))
            return Task.FromResult<IReadOnlyList<string>>(new List<string>());

        var directories = Directory.GetDirectories(versionsDir);
        var versions = directories
            .Select(Path.GetFileName)
            .Where(v => !string.IsNullOrEmpty(v))
            .OrderByDescending(v => v)
            .ToList() as IReadOnlyList<string>;

        return Task.FromResult(versions ?? new List<string>());
    }

    private async Task<string> ResolveVersionAsync(string? version)
    {
        if (string.IsNullOrEmpty(version))
            return await GetLatestVersionAsync();
        return version;
    }

    private string GetLocalFilePath(string version, string set, string fileName)
    {
        return Path.Combine(_baseDataDir, "versions", version, set, fileName);
    }

    private bool TryGetFromMemoryCache<T>(string version, string key, out T value)
    {
        lock (_cacheLock)
        {
            if (_memoryCache.TryGetValue(version, out var versionCache) &&
                versionCache.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
            }
            value = default!;
            return false;
        }
    }

    private void SetMemoryCache<T>(string version, string key, T value)
    {
        lock (_cacheLock)
        {
            if (!_memoryCache.TryGetValue(version, out var versionCache))
            {
                versionCache = new Dictionary<string, object>();
                _memoryCache[version] = versionCache;
            }
            versionCache[key] = value!;
        }
    }

    private async Task<T> LoadFromLocalFileAsync<T>(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? Activator.CreateInstance<T>();
    }

    private async Task SaveToLocalFileAsync<T>(string filePath, T data)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    private List<Champion> ParseChampions(JsonElement data)
    {
        var champions = new List<Champion>();
        if (data.TryGetProperty("data", out var dataProp))
        {
            foreach (var prop in dataProp.EnumerateObject())
            {
                var champ = prop.Value;
                champions.Add(new Champion
                {
                    Id = prop.Name,
                    Name = champ.GetProperty("name").GetString() ?? "",
                    Cost = champ.TryGetProperty("cost", out var costProp) ? costProp.GetInt32() : 0,
                    Traits = new List<string>(),
                    ImageUrl = $"{DRAGON_BASE}/{_cachedLatestVersion}/img/tft-champion/{prop.Name}.png"
                });
            }
        }
        return champions;
    }

    private List<Item> ParseItems(JsonElement data)
    {
        var items = new List<Item>();
        if (data.TryGetProperty("data", out var dataProp))
        {
            foreach (var prop in dataProp.EnumerateObject())
            {
                var item = prop.Value;
                items.Add(new Item
                {
                    Id = prop.Name,
                    Name = item.GetProperty("name").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    Components = new List<string>(),
                    ImageUrl = $"{DRAGON_BASE}/{_cachedLatestVersion}/img/tft-item/{prop.Name}.png"
                });
            }
        }
        return items;
    }

    private List<Trait> ParseTraits(JsonElement data)
    {
        var traits = new List<Trait>();
        if (data.TryGetProperty("data", out var dataProp))
        {
            foreach (var prop in dataProp.EnumerateObject())
            {
                var trait = prop.Value;
                traits.Add(new Trait
                {
                    Id = prop.Name,
                    Name = trait.GetProperty("name").GetString() ?? "",
                    Breakpoints = new List<int>(),
                    ImageUrl = $"{DRAGON_BASE}/{_cachedLatestVersion}/img/tft-trait/{prop.Name}.png"
                });
            }
        }
        return traits;
    }
}
