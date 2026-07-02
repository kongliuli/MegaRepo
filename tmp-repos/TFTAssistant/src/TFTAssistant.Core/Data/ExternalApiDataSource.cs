using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class ExternalApiDataSource : IGameDataSource
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;
    private readonly ILogger<ExternalApiDataSource>? _logger;

    public ExternalApiDataSource(
        HttpClient httpClient,
        string apiBaseUrl,
        string apiKey,
        ILogger<ExternalApiDataSource>? logger = null)
    {
        _httpClient = httpClient;
        _apiBaseUrl = apiBaseUrl;
        _apiKey = apiKey;
        _logger = logger;
        
        // 配置HTTP客户端
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TFTAssistant/1.0");
        
        // 启用HTTP缓存
        _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
        {
            MaxAge = TimeSpan.FromMinutes(5)
        };
    }

    // 通用HTTP请求方法，包含重试机制
    private async Task<T?> SendRequestAsync<T>(string endpoint, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        int maxRetries = 3;
        int retryDelay = 100; // 初始重试延迟100ms
        
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                // 构建查询字符串
                var queryParams = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"{_apiBaseUrl}/{endpoint}?{queryParams}&apiKey={_apiKey}";
                
                _logger?.LogDebug("Sending request to {Url} (Attempt {Retry}/{MaxRetries})", url, retry + 1, maxRetries);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<T>(content);
                    
                    _logger?.LogDebug("Request successful for {Url}", url);
                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || 
                         (int)response.StatusCode >= 500)
                {
                    // 服务器错误或速率限制，进行重试
                    _logger?.LogWarning("Request failed with status code {StatusCode}, retrying...", response.StatusCode);
                    await Task.Delay(retryDelay, cancellationToken);
                    retryDelay *= 2; // 指数退避
                }
                else
                {
                    // 客户端错误，不重试
                    _logger?.LogError("Request failed with status code {StatusCode}", response.StatusCode);
                    return default;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending request, retrying...");
                await Task.Delay(retryDelay, cancellationToken);
                retryDelay *= 2; // 指数退避
            }
        }
        
        _logger?.LogError("Max retries exceeded for {Endpoint}", endpoint);
        return default;
    }

    // 获取元数据
    public async Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion }
        };
        
        var metadata = await SendRequestAsync<MetaData>("metadata", parameters, cancellationToken);
        
        if (metadata != null)
        {
            _logger?.LogInformation("Successfully fetched metadata for set {SetVersion}", setVersion);
        }
        
        return metadata;
    }

    // 获取阵容数据
    public async Task<IEnumerable<LineupData>> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion }
        };
        
        var lineups = await SendRequestAsync<IEnumerable<LineupData>>("lineups", parameters, cancellationToken);
        
        var result = lineups ?? Enumerable.Empty<LineupData>();
        _logger?.LogInformation("Successfully fetched {Count} lineups for set {SetVersion}", result.Count(), setVersion);
        return result;
    }

    // 获取装备数据
    public async Task<IEnumerable<EquipmentData>> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion }
        };
        
        if (isComponent.HasValue)
        {
            parameters.Add("isComponent", isComponent.Value.ToString());
        }
        
        var equipment = await SendRequestAsync<IEnumerable<EquipmentData>>("equipment", parameters, cancellationToken);
        
        var result = equipment ?? Enumerable.Empty<EquipmentData>();
        _logger?.LogInformation("Successfully fetched {Count} equipment items for set {SetVersion}", result.Count(), setVersion);
        return result;
    }

    // 获取英雄数据
    public async Task<IEnumerable<ChampionData>> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion }
        };
        
        if (cost.HasValue)
        {
            parameters.Add("cost", cost.Value.ToString());
        }
        
        var champions = await SendRequestAsync<IEnumerable<ChampionData>>("champions", parameters, cancellationToken);
        
        var result = champions ?? Enumerable.Empty<ChampionData>();
        _logger?.LogInformation("Successfully fetched {Count} champions for set {SetVersion}", result.Count(), setVersion);
        return result;
    }

    // 获取英雄分析数据
    public async Task<ChampionAnalysis?> GetChampionAnalysisAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion },
            { "championId", championId }
        };
        
        var analysis = await SendRequestAsync<ChampionAnalysis>("champions/analysis", parameters, cancellationToken);
        
        if (analysis != null)
        {
            _logger?.LogInformation("Successfully fetched analysis for champion {ChampionId}", championId);
        }
        
        return analysis;
    }

    // 获取装备推荐数据
    public async Task<IEnumerable<EquipmentRecommendation>> GetEquipmentRecommendationsAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion },
            { "championId", championId }
        };
        
        var recommendations = await SendRequestAsync<IEnumerable<EquipmentRecommendation>>("equipment/recommendations", parameters, cancellationToken);
        
        var result = recommendations ?? Enumerable.Empty<EquipmentRecommendation>();
        _logger?.LogInformation("Successfully fetched {Count} equipment recommendations for champion {ChampionId}", result.Count(), championId);
        return result;
    }

    // 获取Meta分析数据
    public async Task<MetaTierAnalysis?> GetMetaTierAnalysisAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "setVersion", setVersion }
        };
        
        var analysis = await SendRequestAsync<MetaTierAnalysis>("meta/analysis", parameters, cancellationToken);
        
        if (analysis != null)
        {
            _logger?.LogInformation("Successfully fetched meta analysis for set {SetVersion}", setVersion);
        }
        
        return analysis;
    }
}
