namespace TFTAssistant.Core.Models.Game;

public sealed class GameStateDiff
{
    public bool HasBoardChanged { get; init; }
    public bool HasBenchChanged { get; init; }
    public bool HasShopChanged { get; init; }
    public bool HasGoldChanged { get; init; }
    public bool HasHealthChanged { get; init; }
    public bool HasLevelChanged { get; init; }
    public bool HasRoundChanged { get; init; }
    public bool HasAugmentPicked { get; init; }
    public bool HasChanges => HasBoardChanged || HasBenchChanged || HasShopChanged || HasGoldChanged || HasHealthChanged || HasLevelChanged || HasRoundChanged || HasAugmentPicked;

    public IReadOnlyList<BoardUnit>? NewBoard { get; init; }
    public IReadOnlyList<BenchUnit>? NewBench { get; init; }
    public IReadOnlyList<ShopUnit>? NewShop { get; init; }
    public int? OldGold { get; init; }
    public int? NewGold { get; init; }
    public double? OldHealth { get; init; }
    public double? NewHealth { get; init; }
    public int? OldLevel { get; init; }
    public int? NewLevel { get; init; }
    public Augment? NewAugment { get; init; }

    public static GameStateDiff Compute(GameState? oldState, GameState newState)
    {
        if (oldState is null)
            return new GameStateDiff
            {
                HasBoardChanged = true,
                HasBenchChanged = true,
                HasShopChanged = true,
                HasGoldChanged = true,
                HasHealthChanged = true,
                HasLevelChanged = true,
                HasRoundChanged = true,
                NewBoard = newState.ActivePlayer.Board,
                NewBench = newState.ActivePlayer.Bench,
                NewShop = newState.ActivePlayer.Shop,
                NewGold = newState.ActivePlayer.CurrentGold,
                NewHealth = newState.ActivePlayer.Health,
                NewLevel = newState.ActivePlayer.Level
            };

        var old = oldState.ActivePlayer;
        var cur = newState.ActivePlayer;

        return new GameStateDiff
        {
            HasBoardChanged = !old.Board.SequenceEqual(cur.Board),
            HasBenchChanged = !old.Bench.SequenceEqual(cur.Bench),
            HasShopChanged = !old.Shop.SequenceEqual(cur.Shop),
            HasGoldChanged = old.CurrentGold != cur.CurrentGold,
            HasHealthChanged = Math.Abs(old.Health - cur.Health) > 0.01,
            HasLevelChanged = old.Level != cur.Level,
            HasRoundChanged = oldState.GameInfo.Round != newState.GameInfo.Round,
            HasAugmentPicked = newState.Augments.Count > oldState.Augments.Count,
            NewBoard = cur.Board,
            NewBench = cur.Bench,
            NewShop = cur.Shop,
            OldGold = old.CurrentGold,
            NewGold = cur.CurrentGold,
            OldHealth = old.Health,
            NewHealth = cur.Health,
            OldLevel = old.Level,
            NewLevel = cur.Level,
            NewAugment = newState.Augments.Count > oldState.Augments.Count
                ? newState.Augments[^1] : null
        };
    }
}
