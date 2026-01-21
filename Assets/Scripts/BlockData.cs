using UnityEngine;

/// <summary>
/// Data structure to hold block color type information
/// </summary>
[System.Serializable]
public class BlockColorData
{
    public int ColorID;
    public Sprite DefaultSprite;
    public Sprite Icon1Sprite;  // Group size > A
    public Sprite Icon2Sprite;  // Group size > B
    public Sprite Icon3Sprite;  // Group size > C
}

/// <summary>
/// Grid position data structure
/// </summary>
public struct GridPosition
{
    public int Row;
    public int Column;

    public GridPosition(int row, int col)
    {
        Row = row;
        Column = col;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is GridPosition)) return false;
        GridPosition other = (GridPosition)obj;
        return Row == other.Row && Column == other.Column;
    }

    public override int GetHashCode()
    {
        return Row * 1000 + Column;
    }

    public static bool operator ==(GridPosition a, GridPosition b)
    {
        return a.Row == b.Row && a.Column == b.Column;
    }

    public static bool operator !=(GridPosition a, GridPosition b)
    {
        return !(a == b);
    }
}
