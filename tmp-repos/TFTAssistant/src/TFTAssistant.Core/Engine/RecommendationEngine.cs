using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 推荐引擎
/// 整合各种建议功能，提供综合推荐
/// </summary>
public sealed class RecommendationEngine : IRecommendationEngine
{
    private readonly ICompMatcher _compMatcher;
    private readonly IItemAdvisor _itemAdvisor;
    private readonly IEconomyAdvisor _economyAdvisor;
    private readonly IAugmentAdvisor _augmentAdvisor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RecommendationEngine> _logger;

    public RecommendationEngine(
        ICompMatcher compMatcher,
        IItemAdvisor itemAdvisor,
        IEconomyAdvisor economyAdvisor,
        IAugmentAdvisor augmentAdvisor,
        IMemoryCache cache,
        ILogger<RecommendationEngine> logger)
    {
        _compMatcher = compMatcher;
        _itemAdvisor = itemAdvisor;
        _economyAdvisor = economyAdvisor;
        _augmentAdvisor = augmentAdvisor;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RecommendationSet> GetRecommendationsAsync(GameState state, CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(state);
        
        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out RecommendationSet cachedResult))
        {
            _logger.LogDebug("推荐结果从缓存获取");
            return cachedResult;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            _logger.LogInformation("开始生成推荐，游戏阶段: {Stage}, 回合: {Round}, 等级: {Level}, 金币: {Gold}, 血量: {Health}", 
                state.GameInfo.Stage, state.GameInfo.Round, state.ActivePlayer.Level, state.ActivePlayer.TotalGold, state.ActivePlayer.Health);
            
            // 预计算可用组件，避免在异步任务中重复计算
            var availableComponents = GetAvailableComponents(state);
            
            // 并行获取各种建议
            var compTask = _compMatcher.MatchAsync(
                state.BoardUnits,
                state.BenchUnits,
                state.ActivePlayer.Augments,
                cancellationToken);

            var itemTask = _itemAdvisor.AdviseAsync(
                state.BoardUnits,
                availableComponents,
                cancellationToken);

            var augmentTask = _augmentAdvisor.RateAsync(
                state.AvailableAugments,
                state.ActivePlayer.Augments,
                state.BoardUnits,
                cancellationToken);

            // 获取经济提示（同步操作）
            var economyHint = _economyAdvisor.Advise(
                state.ActivePlayer.TotalGold,
                state.ActivePlayer.Level,
                state.ActivePlayer.Health,
                state.ActivePlayer.WinStreak > 0,
                state.ActivePlayer.LoseStreak > 0,
                state.GameInfo.Stage);

            // 等待所有异步任务完成，设置超时
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(400)); // 400ms超时，留100ms缓冲
            
            try
            {
                await Task.WhenAll(compTask, itemTask, augmentTask).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("推荐生成超时，返回部分结果");
                // 继续执行，使用已完成的任务结果
            }

            // 获取结果，处理可能的任务取消
            var compSuggestions = compTask.IsCompletedSuccessfully ? await compTask : new List<CompSuggestion>();
            var itemSuggestions = itemTask.IsCompletedSuccessfully ? await itemTask : new List<ItemSuggestion>();
            var augmentRatings = augmentTask.IsCompletedSuccessfully ? await augmentTask : new List<AugmentRating>();

            var result = new RecommendationSet
            {
                CompSuggestions = compSuggestions,
                ItemSuggestions = itemSuggestions,
                EconomyHint = economyHint,
                AugmentRatings = augmentRatings
            };

            stopwatch.Stop();
            
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("推荐生成完成，耗时: {ElapsedMs}ms, 阵容建议: {CompCount}, 装备建议: {ItemCount}, 强化符文评分: {AugmentCount}", 
                elapsedMs, compSuggestions.Count, itemSuggestions.Count, augmentRatings.Count);

            // 缓存结果，有效期根据生成时间动态调整
            var cacheDuration = Math.Max(2, Math.Min(10, 10 - elapsedMs / 100)); // 2-10秒
            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(cacheDuration));
            _logger.LogDebug("推荐结果已缓存，有效期: {CacheDuration}秒", cacheDuration);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("推荐生成被取消");
            return new RecommendationSet
            {
                CompSuggestions = new List<CompSuggestion>(),
                ItemSuggestions = new List<ItemSuggestion>(),
                EconomyHint = null,
                AugmentRatings = new List<AugmentRating>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成推荐失败");
            return new RecommendationSet
            {
                CompSuggestions = new List<CompSuggestion>(),
                ItemSuggestions = new List<ItemSuggestion>(),
                EconomyHint = null,
                AugmentRatings = new List<AugmentRating>()
            };
        }
    }

    private string GenerateCacheKey(GameState state)
    {
        // 生成唯一的缓存键，基于游戏状态的关键信息
        var keyBuilder = new System.Text.StringBuilder();
        keyBuilder.Append($"stage:{state.GameInfo.Stage}");
        keyBuilder.Append($"_gold:{state.ActivePlayer.TotalGold}");
        keyBuilder.Append($"_level:{state.ActivePlayer.Level}");
        keyBuilder.Append($"_health:{state.ActivePlayer.Health}");
        keyBuilder.Append($"_winStreak:{state.ActivePlayer.WinStreak}");
        keyBuilder.Append($"_loseStreak:{state.ActivePlayer.LoseStreak}");
        
        // 添加棋盘单位信息
        foreach (var unit in state.BoardUnits.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"_board:{unit.ChampionName}:{unit.StarLevel}");
        }
        
        // 添加备战席单位信息
        foreach (var unit in state.BenchUnits.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"_bench:{unit.ChampionName}:{unit.StarLevel}");
        }
        
        // 添加已选强化符文
        foreach (var augment in state.ActivePlayer.Augments.OrderBy(a => a.Id))
        {
            keyBuilder.Append($"_augment:{augment.Id}");
        }
        
        // 添加可用强化符文
        if (state.AvailableAugments != null)
        {
            foreach (var augment in state.AvailableAugments.OrderBy(a => a.Id))
            {
                keyBuilder.Append($"_availableAugment:{augment.Id}");
            }
        }
        
        return keyBuilder.ToString();
    }

    private static List<ItemComponent> GetAvailableComponents(GameState state)
    {
        var components = new List<ItemComponent>();
        
        // 从备战席和棋盘上的单位提取未合成的装备组件
        foreach (var unit in state.BoardUnits.Concat(state.BenchUnits))
        {
            foreach (var item in unit.Items)
            {
                // 假设 item 是组件时，直接添加
                if (IsItemComponent(item))
                {
                    components.Add(new ItemComponent { Id = item });
                }
            }
        }
        
        // 从玩家的装备库中提取组件（如果有）
        if (state.ActivePlayer?.Items != null)
        {
            foreach (var item in state.ActivePlayer.Items)
            {
                if (IsItemComponent(item))
                {
                    components.Add(new ItemComponent { Id = item });
                }
            }
        }
        
        return components;
    }
    
    private static bool IsItemComponent(string itemId)
    {
        // 常见的装备组件ID列表
        var componentIds = new HashSet<string>
        {
            "Tear", "Sword", "Bow", "Rod", "Belt", "Cloak", "Chain", "Vamp", "Glove"
        };
        
        return componentIds.Contains(itemId);
    }
}
