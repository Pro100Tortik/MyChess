using System.Collections.Generic;
using UnityEngine;

public class RookMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int currentPos = piece.Data.BoardPosition;

        for (int i = 0; i < 8; i++)
        {
            if (i != currentPos.y) moves.Add(new Vector2Int(currentPos.x, i));
            if (i != currentPos.x) moves.Add(new Vector2Int(i, currentPos.y));
        }

        return FilterValidMoves(FilterBlockedMoves(moves));
    }
}