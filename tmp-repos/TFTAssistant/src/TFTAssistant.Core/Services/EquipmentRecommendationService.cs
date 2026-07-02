using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Services;

/// <summary>
/// 装备推荐服务
/// 提供基于棋盘状态、经济状况和装备获取概率的智能装备推荐
/// </summary>
public sealed class EquipmentRecommendationService
{
    private readonly IBigDataService _bigDataService;
    private readonly IStaticDataProvider _staticDataProvider;
    private readonly EquipmentCalculationService _calculationService;
    private readonly ILogger<EquipmentRecommendationService> _logger;

    public EquipmentRecommendationService(
        IBigDataService bigDataService,
        IStaticDataProvider staticDataProvider,
        EquipmentCalculationService calculationService,
        ILogger<EquipmentRecommendationService> logger)
    {
        _bigDataService = bigDataService;
        _staticDataProvider = staticDataProvider;
        _calculationService = calculationService;
        _logger = logger;
    }

    /// <summary>
    /// 获取基于当前棋盘状态的装备推荐
    /// </summary>
    /// <param name="board">当前棋盘</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gameState">游戏状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    public async Task<List<ItemSuggestion>> GetBoardBasedRecommendationsAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents,
        GameState gameState,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始获取基于棋盘状态的装备推荐");

            var recommendations = new List<ItemSuggestion>();
            var usedComponents = new HashSet<string>();
            var setVersion = gameState.GameInfo?.SetNumber ?? "set13";

            // 按优先级排序英雄：3星 > 2星 > 1星，已装备数量少的优先
            var sortedUnits = board
                .OrderByDescending(u => u.StarLevel)
                .ThenBy(u => u.Items.Count)
                .ToList();

            // 为每个英雄生成推荐
            foreach (var unit in sortedUnits)
            {
                var heroRecommendations = await GetHeroEquipmentRecommendationsAsync(
                    unit.ChampionName,
                    availableComponents.Where(c => !usedComponents.Contains(c)).ToList(),
                    gameState,
                    cancellationToken);

                // 选择优先级最高的推荐
                var bestRecommendation = heroRecommendations.FirstOrDefault();
                if (bestRecommendation != null)
                {
                    recommendations.Add(bestRecommendation);
                    
                    // 标记组件为已使用
                    foreach (var component in bestRecommendation.AvailableComponents)
                    {
                        usedComponents.Add(component);
                    }
                }
            }

            // 按优先级排序推荐
            recommendations.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            _logger.LogInformation("生成了 {Count} 个基于棋盘状态的装备推荐", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取基于棋盘状态的装备推荐失败");
            return new List<ItemSuggestion>();
        }
    }

    /// <summary>
    /// 获取英雄的装备推荐
    /// </summary>
    /// <param name="championName">英雄名称</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gameState">游戏状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    private async Task<List<ItemSuggestion>> GetHeroEquipmentRecommendationsAsync(
        string championName,
        List<ItemComponent> availableComponents,
        GameState gameState,
        CancellationToken cancellationToken = default)
    {
        var setVersion = gameState.GameInfo?.SetNumber ?? "set13";
        var gold = gameState.Player?.TotalGold ?? 0;
        var gameStage = gameState.GameInfo?.Stage ?? "";

        // 创建计算上下文
        var context = new EquipmentCalculationContext
        {
            SetVersion = setVersion,
            TargetChampion = championName,
            AvailableComponents = availableComponents,
            Gold = gold,
            GameStage = gameStage
        };

        // 生成推荐
        return await _calculationService.GenerateEquipmentRecommendationsAsync(context, cancellationToken);
    }

    /// <summary>
    /// 获取装备合成路径建议
    /// </summary>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>合成路径建议</returns>
    public async Task<List<SynthesisPathSuggestion>> GetSynthesisPathSuggestionsAsync(
        IReadOnlyList<ItemComponent> availableComponents,
        string setVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始获取装备合成路径建议");

            // 获取装备数据
            var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, false, cancellationToken);
            var staticItems = await _staticDataProvider.GetItemsAsync(setVersion, cancellationToken);
            var itemMap = staticItems.ToDictionary(i => i.Id, i => i);

            var suggestions = new List<SynthesisPathSuggestion>();

            // 分析所有可能的合成路径
            foreach (var equipment in equipmentData)
            {
                if (itemMap.TryGetValue(equipment.Id, out var item))
                {
                    // 检查组件是否可用
                    var availableComponentIds = availableComponents.Select(c => c.Id).ToHashSet();
                    var requiredComponents = equipment.Components;
                    var missingComponents = requiredComponents.Where(c => !availableComponentIds.Contains(c)).ToList();
                    var availableRequiredComponents = requiredComponents.Where(c => availableComponentIds.Contains(c)).ToList();

                    // 计算合成路径的可行性
                    double feasibility = availableRequiredComponents.Count / (double)requiredComponents.Count;
                    
                    // 计算合成路径的价值
                    var context = new EquipmentCalculationContext
                    {
                        SetVersion = setVersion,
                        TargetChampion = "",
                        AvailableComponents = availableComponents,
                        Gold = 0
                    };
                    
                    double value = _calculationService.CalculateEquipmentValue(equipment, context);
                    int priority = _calculationService.CalculateEquipmentPriority(equipment, context);

                    suggestions.Add(new SynthesisPathSuggestion
                    {
                        Item = item,
                        RequiredComponents = requiredComponents,
                        AvailableComponents = availableRequiredComponents,
                        MissingComponents = missingComponents,
                        Feasibility = feasibility,
                        Value = value,
                        Priority = priority,
                        WinRate = equipment.WinRate,
                        Top4Rate = equipment.Top4Rate,
                        Top1Rate = equipment.Top1Rate,
                        EconomicValue = equipment.EconomicValue
                    });
                }
            }

            // 按优先级和可行性排序
            suggestions.Sort((a, b) =>
            {
                int priorityComparison = a.Priority.CompareTo(b.Priority);
                if (priorityComparison != 0)
                    return priorityComparison;

                int feasibilityComparison = b.Feasibility.CompareTo(a.Feasibility);
                if (feasibilityComparison != 0)
                    return feasibilityComparison;

                return b.Value.CompareTo(a.Value);
            });

            _logger.LogInformation("生成了 {Count} 个装备合成路径建议", suggestions.Count);
            return suggestions.Take(10).ToList(); // 返回前10个建议
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取装备合成路径建议失败");
            return new List<SynthesisPathSuggestion>();
        }
    }

    /// <summary>
    /// 考虑经济状况的装备推荐
    /// </summary>
    /// <param name="board">当前棋盘</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gold">当前经济</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    public async Task<List<ItemSuggestion>> GetEconomyConsideredRecommendationsAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents,
        int gold,
        string setVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始获取考虑经济状况的装备推荐");

            var recommendations = new List<ItemSuggestion>();
            var usedComponents = new HashSet<string>();

            // 按优先级排序英雄：3星 > 2星 > 1星，已装备数量少的优先
            var sortedUnits = board
                .OrderByDescending(u => u.StarLevel)
                .ThenBy(u => u.Items.Count)
                .ToList();

            // 为每个英雄生成推荐
            foreach (var unit in sortedUnits)
            {
                var heroRecommendations = await GetEconomyBasedHeroRecommendationsAsync(
                    unit.ChampionName,
                    availableComponents.Where(c => !usedComponents.Contains(c)).ToList(),
                    gold,
                    setVersion,
                    cancellationToken);

                // 选择优先级最高的推荐
                var bestRecommendation = heroRecommendations.FirstOrDefault();
                if (bestRecommendation != null)
                {
                    recommendations.Add(bestRecommendation);
                    
                    // 标记组件为已使用
                    foreach (var component in bestRecommendation.AvailableComponents)
                    {
                        usedComponents.Add(component);
                    }
                }
            }

            // 按优先级排序推荐
            recommendations.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            _logger.LogInformation("生成了 {Count} 个考虑经济状况的装备推荐", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取考虑经济状况的装备推荐失败");
            return new List<ItemSuggestion>();
        }
    }

    /// <summary>
    /// 获取考虑经济状况的英雄装备推荐
    /// </summary>
    /// <param name="championName">英雄名称</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gold">当前经济</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    private async Task<List<ItemSuggestion>> GetEconomyBasedHeroRecommendationsAsync(
        string championName,
        List<ItemComponent> availableComponents,
        int gold,
        string setVersion,
        CancellationToken cancellationToken = default)
    {
        // 创建计算上下文
        var context = new EquipmentCalculationContext
        {
            SetVersion = setVersion,
            TargetChampion = championName,
            AvailableComponents = availableComponents,
            Gold = gold
        };

        // 生成推荐
        var recommendations = await _calculationService.GenerateEquipmentRecommendationsAsync(context, cancellationToken);

        // 根据经济状况调整推荐
        if (gold < 10) // 经济紧张
        {
            // 优先推荐低成本装备
            recommendations = recommendations.Where(r => r.Item.Components.Count <= 2).ToList();
        }
        else if (gold > 50) // 经济充裕
        {
            // 可以考虑更多高价值装备
            // 这里可以添加更多逻辑
        }

        return recommendations;
    }

    /// <summary>
    /// 分析装备获取概率并生成推荐
    /// </summary>
    /// <param name="board">当前棋盘</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gameState">游戏状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    public async Task<List<ItemSuggestion>> GetProbabilityBasedRecommendationsAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents,
        GameState gameState,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始获取基于装备获取概率的推荐");

            var recommendations = new List<ItemSuggestion>();
            var usedComponents = new HashSet<string>();
            var setVersion = gameState.GameInfo?.SetNumber ?? "set13";

            // 按优先级排序英雄：3星 > 2星 > 1星，已装备数量少的优先
            var sortedUnits = board
                .OrderByDescending(u => u.StarLevel)
                .ThenBy(u => u.Items.Count)
                .ToList();

            // 为每个英雄生成推荐
            foreach (var unit in sortedUnits)
            {
                var heroRecommendations = await GetProbabilityBasedHeroRecommendationsAsync(
                    unit.ChampionName,
                    availableComponents.Where(c => !usedComponents.Contains(c)).ToList(),
                    gameState,
                    cancellationToken);

                // 选择优先级最高的推荐
                var bestRecommendation = heroRecommendations.FirstOrDefault();
                if (bestRecommendation != null)
                {
                    recommendations.Add(bestRecommendation);
                    
                    // 标记组件为已使用
                    foreach (var component in bestRecommendation.AvailableComponents)
                    {
                        usedComponents.Add(component);
                    }
                }
            }

            // 按优先级排序推荐
            recommendations.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            _logger.LogInformation("生成了 {Count} 个基于装备获取概率的推荐", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取基于装备获取概率的推荐失败");
            return new List<ItemSuggestion>();
        }
    }

    /// <summary>
    /// 获取基于装备获取概率的英雄装备推荐
    /// </summary>
    /// <param name="championName">英雄名称</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="gameState">游戏状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    private async Task<List<ItemSuggestion>> GetProbabilityBasedHeroRecommendationsAsync(
        string championName,
        List<ItemComponent> availableComponents,
        GameState gameState,
        CancellationToken cancellationToken = default)
    {
        var setVersion = gameState.GameInfo?.SetNumber ?? "set13";
        var gold = gameState.Player?.TotalGold ?? 0;
        var gameStage = gameState.GameInfo?.Stage ?? "";

        // 创建计算上下文
        var context = new EquipmentCalculationContext
        {
            SetVersion = setVersion,
            TargetChampion = championName,
            AvailableComponents = availableComponents,
            Gold = gold,
            GameStage = gameStage
        };

        // 生成推荐
        var recommendations = await _calculationService.GenerateEquipmentRecommendationsAsync(context, cancellationToken);

        // 根据游戏阶段调整推荐（考虑装备获取概率）
        if (gameStage.StartsWith("1")) // 游戏初期
        {
            // 优先推荐容易获取的装备
            // 这里可以添加更多逻辑
        }
        else if (gameStage.StartsWith("4")) // 游戏后期
        {
            // 可以考虑更复杂的装备组合
            // 这里可以添加更多逻辑
        }

        return recommendations;
    }
}

/// <summary>
/// 合成路径建议
/// </summary>
public sealed class SynthesisPathSuggestion
{
    /// <summary>
    /// 装备
    /// </summary>
    public required Item Item { get; init; }

    /// <summary>
    /// 所需组件
    /// </summary>
    public required IReadOnlyList<string> RequiredComponents { get; init; }

    /// <summary>
    /// 可用组件
    /// </summary>
    public required List<string> AvailableComponents { get; init; }

    /// <summary>
    /// 缺失组件
    /// </summary>
    public required List<string> MissingComponents { get; init; }

    /// <summary>
    /// 可行性 (0-1)
    /// </summary>
    public double Feasibility { get; init; }

    /// <summary>
    /// 装备价值
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// 优先级 (1-5，1最高)
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// 胜率
    /// </summary>
    public double WinRate { get; init; }

    /// <summary>
    /// 前四率
    /// </summary>
    public double Top4Rate { get; init; }

    /// <summary>
    /// 登顶率
    /// </summary>
    public double Top1Rate { get; init; }

    /// <summary>
    /// 经济价值
    /// </summary>
    public double EconomicValue { get; init; }
}
