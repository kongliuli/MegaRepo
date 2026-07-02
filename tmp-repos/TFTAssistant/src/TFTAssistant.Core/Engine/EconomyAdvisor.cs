using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 经济顾问
/// 提供经济策略建议
/// </summary>
public sealed class EconomyAdvisor : IEconomyAdvisor
{
    public EconomyHint Advise(int gold, int level, int health, bool winStreak, bool loseStreak, int stage)
    {
        // 计算连胜/连败状态
        int streakBonus = CalculateStreakBonus(winStreak, loseStreak);
        // 计算经济效率
        double economyEfficiency = CalculateEconomyEfficiency(gold, level, stage);
        // 计算生存压力
        int survivalPressure = CalculateSurvivalPressure(health, stage);

        // 前期（1-3阶段）
        if (stage <= 3)
        {
            if (gold >= 50)
            {
                return new EconomyHint
                {
                    Title = "保持经济优势",
                    Description = "当前金币充足，继续保持 50 金吃利息，优先升级人口到 6 级",
                    Urgency = HintUrgency.Low,
                    Reasoning = "前期达到 50 金是经济优势的关键，利用利息快速提升人口"
                };
            }
            else if (gold >= 30)
            {
                return new EconomyHint
                {
                    Title = "积累经济",
                    Description = "继续积累金币，争取尽快达到 50 金吃利息，适当购买核心英雄",
                    Urgency = HintUrgency.Low,
                    Reasoning = "距离 50 金目标不远，建议保持克制，只购买必要的核心英雄"
                };
            }
            else if (health <= 50)
            {
                return new EconomyHint
                {
                    Title = "适度提升强度",
                    Description = "生命值较低，建议适度消费提升阵容强度，同时保持经济增长",
                    Urgency = HintUrgency.Medium,
                    Reasoning = "前期生命值过低会增加后期压力，需要在经济和强度之间找到平衡"
                };
            }
            else
            {
                return new EconomyHint
                {
                    Title = "谨慎消费",
                    Description = "当前金币较少，建议谨慎消费，优先购买必要的核心英雄",
                    Urgency = HintUrgency.Medium,
                    Reasoning = "前期经济基础至关重要，避免过度消费影响后续发展"
                };
            }
        }
        // 中期（4-5阶段）
        else if (stage <= 5)
        {
            if (health <= 30)
            {
                return new EconomyHint
                {
                    Title = "紧急提升强度",
                    Description = "生命值较低，建议果断消费金币提升阵容强度，保住连胜或止损",
                    Urgency = HintUrgency.High,
                    Reasoning = "中期是决定游戏走向的关键时期，生命值过低需要立即提升强度"
                };
            }
            else if (gold >= 50)
            {
                if (level < 7)
                {
                    return new EconomyHint
                    {
                        Title = "优先升级人口",
                        Description = "保持经济优势，优先升级到 7 级，扩大阵容规模",
                        Urgency = HintUrgency.Low,
                        Reasoning = "7 级是中期的重要节点，能够容纳更多高费英雄，提升阵容强度"
                    };
                }
                else
                {
                    return new EconomyHint
                    {
                        Title = "稳步提升",
                        Description = "保持经济优势，有计划地提升阵容质量，寻找核心英雄三星",
                        Urgency = HintUrgency.Low,
                        Reasoning = "经济状况良好，可以在保持利息的同时提升阵容质量"
                    };
                }
            }
            else if (streakBonus > 0)
            {
                return new EconomyHint
                {
                    Title = "利用连胜/连败",
                    Description = $"当前{winStreak ? "连胜" : "连败"}中，建议适度消费保持优势或止损",
                    Urgency = HintUrgency.Medium,
                    Reasoning = $"{winStreak ? "连胜" : "连败"}可以提供额外金币，建议利用这个优势提升阵容"
                };
            }
            else
            {
                return new EconomyHint
                {
                    Title = "平衡经济与强度",
                    Description = "在保持经济的同时，适当提升阵容强度，争取进入前四",
                    Urgency = HintUrgency.Medium,
                    Reasoning = "中期需要在经济和强度之间找到平衡，为后期做准备"
                };
            }
        }
        // 后期（6+阶段）
        else
        {
            if (health <= 20)
            {
                return new EconomyHint
                {
                    Title = "全力冲刺",
                    Description = "生命值危急，建议全力以赴提升阵容强度，不留金币",
                    Urgency = HintUrgency.Critical,
                    Reasoning = "后期生命值过低，需要孤注一掷提升强度，争取翻盘"
                };
            }
            else if (gold >= 30)
            {
                if (level < 9)
                {
                    return new EconomyHint
                    {
                        Title = "升级到 9 级",
                        Description = "利用剩余金币升级到 9 级，寻找五费英雄提升阵容上限",
                        Urgency = HintUrgency.Medium,
                        Reasoning = "9 级是后期的重要节点，能够容纳五费英雄，提升阵容上限"
                    };
                }
                else
                {
                    return new EconomyHint
                    {
                        Title = "优化阵容",
                        Description = "利用剩余金币优化阵容，提升核心英雄星级和装备",
                        Urgency = HintUrgency.Medium,
                        Reasoning = "后期需要将金币投入到提升核心英雄星级和装备上，争取登顶"
                    };
                }
            }
            else
            {
                return new EconomyHint
                {
                    Title = "谨慎运营",
                    Description = "合理使用剩余金币，优先提升关键英雄，保持竞争力",
                    Urgency = HintUrgency.Low,
                    Reasoning = "后期金币有限，需要精准投入到最关键的英雄上"
                };
            }
        }
    }

    private int CalculateStreakBonus(bool winStreak, bool loseStreak)
    {
        // 简化计算，实际应根据连续胜场/败场数计算
        if (winStreak)
            return 1; // 连胜奖励
        if (loseStreak)
            return -1; // 连败补偿
        return 0;
    }

    private double CalculateEconomyEfficiency(int gold, int level, int stage)
    {
        // 计算经济效率：金币/等级/阶段
        return gold / (double)Math.Max(1, level * stage / 10);
    }

    private int CalculateSurvivalPressure(int health, int stage)
    {
        // 计算生存压力：生命值越低，阶段越高，压力越大
        return (100 - health) * stage / 10;
    }
}
