using System.Collections.Generic;
using UnityEngine;

public class BishopMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int currentPos = piece.Data.BoardPosition;

        // Diagonal movements
        for (int i = 1; i < 8; i++)
        {
            moves.Add(new Vector2Int(currentPos.x + i, currentPos.y + i));
            moves.Add(new Vector2Int(currentPos.x + i, currentPos.y - i));
            moves.Add(new Vector2Int(currentPos.x - i, currentPos.y + i));
            moves.Add(new Vector2Int(currentPos.x - i, currentPos.y - i));
        }

        return FilterValidMoves(FilterBlockedMoves(moves));
    }
}