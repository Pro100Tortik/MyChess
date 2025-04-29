using System;
using UnityEngine;

[Serializable]
public class PieceData
{
    public bool HasMoved;
    public PieceType Type;
    public PieceColor Color;
    public Vector2Int BoardPosition;
}
