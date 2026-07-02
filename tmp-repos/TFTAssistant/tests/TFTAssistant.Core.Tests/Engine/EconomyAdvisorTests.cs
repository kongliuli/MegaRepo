using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Engine;

public class EconomyAdvisorTests
{
    private readonly EconomyAdvisor _economyAdvisor;

    public EconomyAdvisorTests()
    {
        _economyAdvisor = new EconomyAdvisor();
    }

    [Fact]
    public void Advise_ShouldReturnKeepEconomicAdvantageHint_WhenEarlyStageWithHighGold()
    {
        // Arrange
        int gold = 50;
        int level = 5;
        int health = 80;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 2;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("保持经济优势", result.Title);
        Assert.Equal(HintUrgency.Low, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnAccumulateEconomyHint_WhenEarlyStageWithMediumGold()
    {
        // Arrange
        int gold = 35;
        int level = 4;
        int health = 90;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 3;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("积累经济", result.Title);
        Assert.Equal(HintUrgency.Low, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnModerateStrengthHint_WhenEarlyStageWithLowHealth()
    {
        // Arrange
        int gold = 25;
        int level = 4;
        int health = 40;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 3;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("适度提升强度", result.Title);
        Assert.Equal(HintUrgency.Medium, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnBeCautiousHint_WhenEarlyStageWithLowGold()
    {
        // Arrange
        int gold = 15;
        int level = 3;
        int health = 80;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 2;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("谨慎消费", result.Title);
        Assert.Equal(HintUrgency.Medium, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnUrgentStrengthHint_WhenMidStageWithLowHealth()
    {
        // Arrange
        int gold = 40;
        int level = 6;
        int health = 25;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 4;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("紧急提升强度", result.Title);
        Assert.Equal(HintUrgency.High, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnPrioritizeLevelUpHint_WhenMidStageWithHighGoldAndLowLevel()
    {
        // Arrange
        int gold = 55;
        int level = 6;
        int health = 70;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 4;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("优先升级人口", result.Title);
        Assert.Equal(HintUrgency.Low, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnSteadyImprovementHint_WhenMidStageWithHighGoldAndHighLevel()
    {
        // Arrange
        int gold = 55;
        int level = 7;
        int health = 70;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 5;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("稳步提升", result.Title);
        Assert.Equal(HintUrgency.Low, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnUseStreakHint_WhenMidStageWithStreak()
    {
        // Arrange
        int gold = 30;
        int level = 6;
        int health = 60;
        bool winStreak = true;
        bool loseStreak = false;
        int stage = 4;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("利用连胜/连败", result.Title);
        Assert.Equal(HintUrgency.Medium, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnFullEffortHint_WhenLateStageWithCriticalHealth()
    {
        // Arrange
        int gold = 40;
        int level = 8;
        int health = 15;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 6;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("全力冲刺", result.Title);
        Assert.Equal(HintUrgency.Critical, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnLevelUpTo9Hint_WhenLateStageWithHighGoldAndLowLevel()
    {
        // Arrange
        int gold = 35;
        int level = 8;
        int health = 50;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 6;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("升级到 9 级", result.Title);
        Assert.Equal(HintUrgency.Medium, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnOptimizeTeamHint_WhenLateStageWithHighGoldAndHighLevel()
    {
        // Arrange
        int gold = 35;
        int level = 9;
        int health = 50;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 6;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("优化阵容", result.Title);
        Assert.Equal(HintUrgency.Medium, result.Urgency);
    }

    [Fact]
    public void Advise_ShouldReturnBeCarefulHint_WhenLateStageWithLowGold()
    {
        // Arrange
        int gold = 20;
        int level = 8;
        int health = 40;
        bool winStreak = false;
        bool loseStreak = false;
        int stage = 6;

        // Act
        var result = _economyAdvisor.Advise(gold, level, health, winStreak, loseStreak, stage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("谨慎运营", result.Title);
        Assert.Equal(HintUrgency.Low, result.Urgency);
    }
}
