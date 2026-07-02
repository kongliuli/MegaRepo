using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 强化符文顾问
/// 对强化符文进行评分和建议
/// </summary>
public sealed class AugmentAdvisor : IAugmentAdvisor
{
    private readonly ILogger<AugmentAdvisor> _logger;
    private readonly IMetaAnalysisService _metaAnalysisService;

    public AugmentAdvisor(
        ILogger<AugmentAdvisor> logger,
        IMetaAnalysisService metaAnalysisService)
    {
        _logger = logger;
        _metaAnalysisService = metaAnalysisService;
    }

    public async Task<List<AugmentRating>> RateAsync(
        IReadOnlyList<Augment> options,
        IReadOnlyList<Augment> existingAugments,
        IReadOnlyList<BoardUnit> boardUnits,
        CancellationToken cancellationToken = default)
    {
        var ratings = new List<AugmentRating>();
        var boardChampions = boardUnits.Select(u => u.ChampionName).ToHashSet();
        var boardTraits = await CalculateBoardTraits(boardUnits, cancellationToken);

        foreach (var option in options)
        {
            double score = 50; // 基础分

            // 规则1: 与当前棋盘英雄的契合度
            double boardSynergy = CalculateBoardSynergy(option, boardChampions, boardTraits);
            score += boardSynergy;

            // 规则2: 与已选符文的协同
            double existingSynergy = CalculateExistingSynergy(option, existingAugments);
            score += existingSynergy;

            // 规则3: 通用强度评级
            double baseRating = await GetBaseRating(option.Id, cancellationToken);
            score += baseRating;

            // 规则4: 游戏阶段适配性
            double stageAdaptability = CalculateStageAdaptability(option, boardUnits.Count);
            score += stageAdaptability;

            score = Math.Clamp(score, 0, 100);

            ratings.Add(new AugmentRating
            {
                AugmentId = option.Id,
                AugmentName = option.Name,
                Score = Math.Round(score, 1),
                Reasoning = GenerateAugmentReasoning(option, boardSynergy, existingSynergy, baseRating, stageAdaptability)
            });
        }

        return ratings.OrderByDescending(r => r.Score).ToList();
    }

    private async Task<Dictionary<string, int>> CalculateBoardTraits(IReadOnlyList<BoardUnit> boardUnits, CancellationToken cancellationToken)
    {
        var traits = new Dictionary<string, int>();
        try
        {
            // 这里应该从静态数据或服务中获取英雄-羁绊映射
            // 简化处理，使用默认映射
            foreach (var unit in boardUnits)
            {
                AddDefaultTraits(unit.ChampionName, traits);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate board traits");
        }
        return traits;
    }

    private void AddDefaultTraits(string championName, Dictionary<string, int> traits)
    {
        // 简化处理，实际需从静态数据获取
        if (championName == "Ahri")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Syndicate"] = traits.GetValueOrDefault("Syndicate", 0) + 1;
        }
        else if (championName == "Akali")
        {
            traits["Assassin"] = traits.GetValueOrDefault("Assassin", 0) + 1;
            traits["Syndicate"] = traits.GetValueOrDefault("Syndicate", 0) + 1;
        }
        else if (championName == "Ashe")
        {
            traits["Marksman"] = traits.GetValueOrDefault("Marksman", 0) + 1;
            traits["Freljord"] = traits.GetValueOrDefault("Freljord", 0) + 1;
        }
        else if (championName == "Draven")
        {
            traits["Gladiator"] = traits.GetValueOrDefault("Gladiator", 0) + 1;
            traits["Noxus"] = traits.GetValueOrDefault("Noxus", 0) + 1;
        }
        else if (championName == "Kai'Sa")
        {
            traits["Assassin"] = traits.GetValueOrDefault("Assassin", 0) + 1;
            traits["Void"] = traits.GetValueOrDefault("Void", 0) + 1;
        }
        else if (championName == "Leona")
        {
            traits["Guardian"] = traits.GetValueOrDefault("Guardian", 0) + 1;
            traits["Dawnbringer"] = traits.GetValueOrDefault("Dawnbringer", 0) + 1;
        }
        else if (championName == "Lux")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Dawnbringer"] = traits.GetValueOrDefault("Dawnbringer", 0) + 1;
        }
        else if (championName == "MissFortune")
        {
            traits["Marksman"] = traits.GetValueOrDefault("Marksman", 0) + 1;
            traits["Bilgewater"] = traits.GetValueOrDefault("Bilgewater", 0) + 1;
        }
        else if (championName == "Sett")
        {
            traits["Brawler"] = traits.GetValueOrDefault("Brawler", 0) + 1;
            traits["Void"] = traits.GetValueOrDefault("Void", 0) + 1;
        }
        else if (championName == "Syndra")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Coven"] = traits.GetValueOrDefault("Coven", 0) + 1;
        }
    }

    private double CalculateBoardSynergy(Augment augment, HashSet<string> boardChampions, Dictionary<string, int> boardTraits)
    {
        double synergyScore = 0;
        
        // 与英雄的契合度
        foreach (var champion in boardChampions)
        {
            if (augment.Id.Contains(champion, StringComparison.OrdinalIgnoreCase))
            {
                synergyScore += 15;
                break;
            }
        }
        
        // 与羁绊的契合度
        foreach (var trait in boardTraits.Keys)
        {
            if (augment.Id.Contains(trait, StringComparison.OrdinalIgnoreCase))
            {
                synergyScore += 10 * (boardTraits[trait] / 3.0); // 基于羁绊激活数量
                break;
            }
        }
        
        // 通用类型契合度
        if (augment.Id.Contains("Mage") && boardTraits.ContainsKey("Mage"))
            synergyScore += 8;
        if (augment.Id.Contains("Assassin") && boardTraits.ContainsKey("Assassin"))
            synergyScore += 8;
        if (augment.Id.Contains("Marksman") && boardTraits.ContainsKey("Marksman"))
            synergyScore += 8;
        if (augment.Id.Contains("Brawler") && boardTraits.ContainsKey("Brawler"))
            synergyScore += 8;
        if (augment.Id.Contains("Guardian") && boardTraits.ContainsKey("Guardian"))
            synergyScore += 8;
        
        return synergyScore;
    }

    private double CalculateExistingSynergy(Augment augment, IReadOnlyList<Augment> existingAugments)
    {
        double synergyScore = 0;
        
        // 同类型符文协同
        string augmentType = GetAugmentType(augment.Id);
        foreach (var existing in existingAugments)
        {
            string existingType = GetAugmentType(existing.Id);
            if (augmentType == existingType && augmentType != "")
            {
                synergyScore += 12;
                break;
            }
        }
        
        // 功能协同
        if (existingAugments.Any(ea => ea.Id.Contains("Gold") && augment.Id.Contains("Gold")))
            synergyScore += 10; // 经济类协同
        if (existingAugments.Any(ea => ea.Id.Contains("Damage") && augment.Id.Contains("Damage")))
            synergyScore += 10; // 输出类协同
        if (existingAugments.Any(ea => ea.Id.Contains("Defense") && augment.Id.Contains("Defense")))
            synergyScore += 10; // 防御类协同
        
        return synergyScore;
    }

    private string GetAugmentType(string augmentId)
    {
        if (augmentId.Contains("Mage")) return "Mage";
        if (augmentId.Contains("Assassin")) return "Assassin";
        if (augmentId.Contains("Marksman")) return "Marksman";
        if (augmentId.Contains("Brawler")) return "Brawler";
        if (augmentId.Contains("Guardian")) return "Guardian";
        if (augmentId.Contains("Gold")) return "Economy";
        if (augmentId.Contains("Damage")) return "Offense";
        if (augmentId.Contains("Defense")) return "Defense";
        return "";
    }

    private async Task<double> GetBaseRating(string augmentId, CancellationToken cancellationToken)
    {
        try
        {
            // 从元数据分析服务获取基础评级
            var augmentRatings = await _metaAnalysisService.GetAugmentRatingsAsync(cancellationToken);
            if (augmentRatings != null && augmentRatings.TryGetValue(augmentId, out double rating))
            {
                return rating;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get augment rating from meta analysis");
        }
        
        // 回退：使用基于符文类型的默认评级
        return GetDefaultAugmentRating(augmentId);
    }

    private double GetDefaultAugmentRating(string augmentId)
    {
        // 基于符文类型的默认评级
        if (augmentId.Contains("Emblem"))
            return 20; // 纹章通常强度较高
        if (augmentId.Contains("Gold"))
            return 18; // 经济类符文
        if (augmentId.Contains("Damage"))
            return 15; // 输出类符文
        if (augmentId.Contains("Defense"))
            return 12; // 防御类符文
        return 10; // 其他符文
    }

    private double CalculateStageAdaptability(Augment augment, int boardSize)
    {
        double adaptabilityScore = 0;
        
        // 前期适配性（小棋盘）
        if (boardSize <= 4)
        {
            if (augment.Id.Contains("Gold"))
                adaptabilityScore += 8; // 前期经济很重要
            if (augment.Id.Contains("Defense"))
                adaptabilityScore += 5; // 前期生存也重要
        }
        // 后期适配性（大棋盘）
        else if (boardSize >= 6)
        {
            if (augment.Id.Contains("Damage"))
                adaptabilityScore += 8; // 后期输出更重要
            if (augment.Id.Contains("Emblem"))
                adaptabilityScore += 6; // 后期纹章价值更高
        }
        
        return adaptabilityScore;
    }

    private string GenerateAugmentReasoning(Augment augment, double boardSynergy, double existingSynergy, double baseRating, double stageAdaptability)
    {
        var reasons = new List<string>();
        
        // 基于棋盘契合度的理由
        if (boardSynergy >= 15)
            reasons.Add("与当前棋盘英雄高度契合");
        else if (boardSynergy >= 8)
            reasons.Add("与当前棋盘有良好契合");
        
        // 基于已选符文协同的理由
        if (existingSynergy >= 12)
            reasons.Add("与已选强化符文有很强的协同效应");
        else if (existingSynergy >= 8)
            reasons.Add("与已选强化符文有较好的协同");
        
        // 基于基础强度的理由
        if (baseRating >= 18)
            reasons.Add("当前版本强度较高的强化符文");
        else if (baseRating >= 12)
            reasons.Add("当前版本强度不错的强化符文");
        
        // 基于阶段适配性的理由
        if (stageAdaptability >= 6)
            reasons.Add("非常适合当前游戏阶段");
        else if (stageAdaptability >= 3)
            reasons.Add("适合当前游戏阶段");
        
        if (reasons.Count == 0)
        {
            reasons.Add("基于当前游戏状态的推荐强化符文");
        }
        
        return string.Join("；", reasons);
    }
}
