namespace TFTAssistant.Core.Models.Game;

public sealed class BoardUnit : IEquatable<BoardUnit>
{
    public required string ChampionName { get; init; }
    public required int StarLevel { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
    public required int Row { get; init; }
    public required int Column { get; init; }
    public bool IsMainCarry { get; set; }

    public bool Equals(BoardUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName
            && StarLevel == other.StarLevel
            && Row == other.Row
            && Column == other.Column
            && Items.SequenceEqual(other.Items);
    }

    public override bool Equals(object? obj) => Equals(obj as BoardUnit);

    public override int GetHashCode() => HashCode.Combine(ChampionName, Row, Column);

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(ChampionName))
            return false;
        
        if (StarLevel < 1 || StarLevel > 3)
            return false;
        
        if (Items == null)
            return false;
        
        if (Row < 0 || Row > 3) // 假设棋盘有 4 行
            return false;
        
        if (Column < 0 || Column > 6) // 假设棋盘有 7 列
            return false;
        
        return true;
    }
}
