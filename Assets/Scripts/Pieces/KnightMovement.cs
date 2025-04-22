using System.Collections.Generic;
using UnityEngine;

public class KnightMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        Vector2Int currentPos = piece.Data.BoardPosition;
        List<Vector2Int> moves = new List<Vector2Int>
        {
            new(currentPos.x + 1, currentPos.y + 2),
            new(currentPos.x + 2, currentPos.y + 1),
            new(currentPos.x + 2, currentPos.y - 1),
            new(currentPos.x + 1, currentPos.y - 2),
            new(currentPos.x - 1, currentPos.y - 2),
            new(currentPos.x - 2, currentPos.y - 1),
            new(currentPos.x - 2, currentPos.y + 1),
            new(currentPos.x - 1, currentPos.y + 2)
        };

        return FilterValidMoves(moves);
    }
}