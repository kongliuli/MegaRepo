using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.BigData;
using TFTAssistant.Core.Models.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace TFTAssistant.Core.Services;

/// <summary>
/// 装备数据服务
/// 负责从外部数据源获取装备数据，整合分析装备的表现数据
/// </summary>
public sealed class EquipmentDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBigDataService _bigDataService;
    private readonly IStaticDataProvider _staticDataProvider;
    private readonly ILogger<EquipmentDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EquipmentDataService(
        IHttpClientFactory httpClientFactory,
        IBigDataService bigDataService,
        IStaticDataProvider staticDataProvider,
        ILogger<EquipmentDataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _bigDataService = bigDataService;
        _staticDataProvider = staticDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// 从外部数据源获取装备数据并整合
    /// </summary>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备数据列表</returns>
    public async Task<IEnumerable<EquipmentData>> FetchAndIntegrateEquipmentDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始获取装备数据: {SetVersion}", setVersion);

            // 1. 获取静态装备数据
            var staticItems = await _staticDataProvider.GetItemsAsync(setVersion, cancellationToken);
            _logger.LogDebug("获取到 {Count} 个静态装备数据", staticItems.Count);

            // 2. 从外部数据源获取装备表现数据
            var equipmentPerformanceData = await FetchEquipmentPerformanceDataAsync(setVersion, cancellationToken);
            _logger.LogDebug("获取到 {Count} 个装备表现数据", equipmentPerformanceData.Count);

            // 3. 整合数据
            var integratedData = IntegrateEquipmentData(staticItems, equipmentPerformanceData);
            _logger.LogDebug("整合后得到 {Count} 个装备数据", integratedData.Count);

            // 4. 保存到数据库
            await _bigDataService.SaveEquipmentDataBatchAsync(integratedData, cancellationToken);
            _logger.LogInformation("装备数据保存完成");

            return integratedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取装备数据失败: {SetVersion}", setVersion);
            throw;
        }
    }

    /// <summary>
    /// 从外部数据源获取装备表现数据
    /// </summary>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备表现数据</returns>
    private async Task<List<EquipmentPerformanceData>> FetchEquipmentPerformanceDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        // 模拟从外部数据源获取装备表现数据
        // 实际实现中，这里应该调用外部 API 或解析数据文件
        var performanceData = new List<EquipmentPerformanceData>();
        var random = new Random();

        // 模拟装备数据
        var equipmentNames = new[]
        {
            "InfinityEdge", "GuinsoosRageblade", "Bloodthirster", "Shojin", "BlueBuff", "JeweledGauntlet",
            "GiantSlayer", "LastWhisper", "GuardianAngel", "Warmogs", "Thornmail", "Redemption",
            "ZekesHerald", "ShurelyasBattlesong", "LocketOfTheIronSolari", "Morellonomicon", "RabadonsDeathcap",
            "VoidStaff", "RunaansHurricane", "StatikkShiv", "QuickSilver", "BrambleVest", "DragonClaw"
        };

        foreach (var name in equipmentNames)
        {
            performanceData.Add(new EquipmentPerformanceData
            {
                EquipmentId = name,
                WinRate = 0.45 + random.NextDouble() * 0.3, // 45% - 75%
                Top4Rate = 0.6 + random.NextDouble() * 0.3, // 60% - 90%
                Top1Rate = 0.2 + random.NextDouble() * 0.4, // 20% - 60%
                PickRate = 0.05 + random.NextDouble() * 0.3, // 5% - 35%
                MatchCount = 5000 + random.Next(15000), // 5000 - 20000
                EconomicValue = 1.0 + random.NextDouble() * 1.0, // 1.0 - 2.0
                EquipmentType = GetRandomEquipmentType(),
                PerformanceByRank = GeneratePerformanceByRank(),
                PerformanceByLineup = GeneratePerformanceByLineup()
            });
        }

        return performanceData;
    }

    /// <summary>
    /// 整合静态装备数据和表现数据
    /// </summary>
    /// <param name="staticItems">静态装备数据</param>
    /// <param name="performanceData">表现数据</param>
    /// <returns>整合后的装备数据</returns>
    private List<EquipmentData> IntegrateEquipmentData(IReadOnlyList<Item> staticItems, List<EquipmentPerformanceData> performanceData)
    {
        var performanceMap = performanceData.ToDictionary(p => p.EquipmentId, p => p);
        var integratedData = new List<EquipmentData>();

        foreach (var item in staticItems)
        {
            if (performanceMap.TryGetValue(item.Id, out var performance))
            {
                integratedData.Add(new EquipmentData
                {
                    Id = item.Id,
                    Name = item.Name,
                    SetVersion = "set13", // 实际应从参数获取
                    Description = item.Description,
                    ImageUrl = item.ImageUrl,
                    IsComponent = item.Type == ItemType.Component,
                    Components = item.Components,
                    WinRate = performance.WinRate,
                    Top4Rate = performance.Top4Rate,
                    Top1Rate = performance.Top1Rate,
                    PickRate = performance.PickRate,
                    EconomicValue = performance.EconomicValue,
                    EquipmentType = performance.EquipmentType,
                    MatchCount = performance.MatchCount,
                    LastUpdated = DateTime.Now
                });
            }
            else
            {
                // 如果没有表现数据，使用默认值
                integratedData.Add(new EquipmentData
                {
                    Id = item.Id,
                    Name = item.Name,
                    SetVersion = "set13", // 实际应从参数获取
                    Description = item.Description,
                    ImageUrl = item.ImageUrl,
                    IsComponent = item.Type == ItemType.Component,
                    Components = item.Components,
                    WinRate = 0.5, // 默认值
                    Top4Rate = 0.65, // 默认值
                    Top1Rate = 0.3, // 默认值
                    PickRate = 0.1, // 默认值
                    EconomicValue = 1.0, // 默认值
                    EquipmentType = item.Type.ToString(),
                    MatchCount = 0,
                    LastUpdated = DateTime.Now
                });
            }
        }

        return integratedData;
    }

    /// <summary>
    /// 计算装备的经济价值
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <returns>经济价值</returns>
    public double CalculateEconomicValue(EquipmentData equipment)
    {
        // 基于装备的胜率、前四率、登顶率和获取难度计算经济价值
        double baseValue = equipment.WinRate * 0.4 + equipment.Top4Rate * 0.3 + equipment.Top1Rate * 0.3;
        
        // 考虑装备类型的价值调整
        double typeMultiplier = equipment.EquipmentType switch
        {
            "Artifact" => 1.5,
            "Radiant" => 1.3,
            "Completed" => 1.0,
            "Component" => 0.5,
            _ => 1.0
        };
        
        // 考虑装备的获取难度
        double availabilityFactor = equipment.IsComponent ? 1.0 : 0.8;
        
        return baseValue * typeMultiplier * availabilityFactor;
    }

    /// <summary>
    /// 分析装备在不同段位中的表现
    /// </summary>
    /// <param name="equipmentId">装备ID</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>段位表现分析</returns>
    public async Task<Dictionary<string, EquipmentRankPerformance>> AnalyzeEquipmentByRankAsync(string equipmentId, string setVersion, CancellationToken cancellationToken = default)
    {
        // 从数据库获取装备数据
        var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, cancellationToken);
        var equipment = equipmentData.FirstOrDefault(e => e.Id == equipmentId);
        
        if (equipment == null)
        {
            _logger.LogWarning("装备 {EquipmentId} 未找到", equipmentId);
            return new Dictionary<string, EquipmentRankPerformance>();
        }
        
        // 模拟不同段位的表现数据
        return new Dictionary<string, EquipmentRankPerformance>
        {
            { "Iron", new EquipmentRankPerformance { WinRate = equipment.WinRate - 0.1, PickRate = equipment.PickRate + 0.1, MatchCount = equipment.MatchCount / 5 } },
            { "Bronze", new EquipmentRankPerformance { WinRate = equipment.WinRate - 0.08, PickRate = equipment.PickRate + 0.08, MatchCount = equipment.MatchCount / 4 } },
            { "Silver", new EquipmentRankPerformance { WinRate = equipment.WinRate - 0.05, PickRate = equipment.PickRate + 0.05, MatchCount = equipment.MatchCount / 3 } },
            { "Gold", new EquipmentRankPerformance { WinRate = equipment.WinRate - 0.02, PickRate = equipment.PickRate + 0.02, MatchCount = equipment.MatchCount / 2 } },
            { "Platinum", new EquipmentRankPerformance { WinRate = equipment.WinRate, PickRate = equipment.PickRate, MatchCount = equipment.MatchCount } },
            { "Diamond", new EquipmentRankPerformance { WinRate = equipment.WinRate + 0.03, PickRate = equipment.PickRate - 0.03, MatchCount = equipment.MatchCount / 2 } },
            { "Master", new EquipmentRankPerformance { WinRate = equipment.WinRate + 0.05, PickRate = equipment.PickRate - 0.05, MatchCount = equipment.MatchCount / 3 } },
            { "Grandmaster", new EquipmentRankPerformance { WinRate = equipment.WinRate + 0.08, PickRate = equipment.PickRate - 0.08, MatchCount = equipment.MatchCount / 4 } },
            { "Challenger", new EquipmentRankPerformance { WinRate = equipment.WinRate + 0.1, PickRate = equipment.PickRate - 0.1, MatchCount = equipment.MatchCount / 5 } }
        };
    }

    /// <summary>
    /// 分析装备在不同阵容中的表现
    /// </summary>
    /// <param name="equipmentId">装备ID</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>阵容表现分析</returns>
    public async Task<Dictionary<string, EquipmentLineupPerformance>> AnalyzeEquipmentByLineupAsync(string equipmentId, string setVersion, CancellationToken cancellationToken = default)
    {
        // 从数据库获取装备数据
        var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, cancellationToken);
        var equipment = equipmentData.FirstOrDefault(e => e.Id == equipmentId);
        
        if (equipment == null)
        {
            _logger.LogWarning("装备 {EquipmentId} 未找到", equipmentId);
            return new Dictionary<string, EquipmentLineupPerformance>();
        }
        
        // 模拟不同阵容的表现数据
        var lineups = new[] { "神龙尊者", "冒险家", "怒翼龙", "玉龙", "金鳞龙", "幽影龙", "魔导士", "强袭炮手" };
        var random = new Random();
        var result = new Dictionary<string, EquipmentLineupPerformance>();
        
        foreach (var lineup in lineups)
        {
            result.Add(lineup, new EquipmentLineupPerformance
            {
                WinRate = equipment.WinRate + (random.NextDouble() - 0.5) * 0.2,
                PickRate = equipment.PickRate + (random.NextDouble() - 0.5) * 0.1,
                MatchCount = random.Next(1000, 5000)
            });
        }
        
        return result;
    }

    /// <summary>
    /// 获取随机装备类型
    /// </summary>
    /// <returns>装备类型</returns>
    private string GetRandomEquipmentType()
    {
        var types = new[] { "Artifact", "Radiant", "Completed", "Component" };
        var random = new Random();
        return types[random.Next(types.Length)];
    }

    /// <summary>
    /// 生成不同段位的表现数据
    /// </summary>
    /// <returns>段位表现数据</returns>
    private Dictionary<string, EquipmentRankPerformance> GeneratePerformanceByRank()
    {
        var ranks = new[] { "Iron", "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Grandmaster", "Challenger" };
        var random = new Random();
        var result = new Dictionary<string, EquipmentRankPerformance>();
        
        foreach (var rank in ranks)
        {
            result.Add(rank, new EquipmentRankPerformance
            {
                WinRate = 0.4 + random.NextDouble() * 0.3,
                PickRate = 0.05 + random.NextDouble() * 0.3,
                MatchCount = 1000 + random.Next(4000)
            });
        }
        
        return result;
    }

    /// <summary>
    /// 生成不同阵容的表现数据
    /// </summary>
    /// <returns>阵容表现数据</returns>
    private Dictionary<string, EquipmentLineupPerformance> GeneratePerformanceByLineup()
    {
        var lineups = new[] { "神龙尊者", "冒险家", "怒翼龙", "玉龙", "金鳞龙" };
        var random = new Random();
        var result = new Dictionary<string, EquipmentLineupPerformance>();
        
        foreach (var lineup in lineups)
        {
            result.Add(lineup, new EquipmentLineupPerformance
            {
                WinRate = 0.4 + random.NextDouble() * 0.3,
                PickRate = 0.05 + random.NextDouble() * 0.3,
                MatchCount = 1000 + random.Next(4000)
            });
        }
        
        return result;
    }
}

/// <summary>
/// 装备表现数据
/// </summary>
public sealed class EquipmentPerformanceData
{
    public string EquipmentId { get; init; } = "";
    public double WinRate { get; init; }
    public double Top4Rate { get; init; }
    public double Top1Rate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public double EconomicValue { get; init; }
    public string EquipmentType { get; init; } = "";
    public Dictionary<string, EquipmentRankPerformance> PerformanceByRank { get; init; } = new();
    public Dictionary<string, EquipmentLineupPerformance> PerformanceByLineup { get; init; } = new();
}

/// <summary>
/// 装备段位表现
/// </summary>
public sealed class EquipmentRankPerformance
{
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
}

/// <summary>
/// 装备阵容表现
/// </summary>
public sealed class EquipmentLineupPerformance
{
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
}
