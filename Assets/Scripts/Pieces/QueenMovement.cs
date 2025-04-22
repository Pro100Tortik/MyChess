using System.Collections.Generic;
using UnityEngine;

public class QueenMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int currentPos = piece.Data.BoardPosition;

        // Combine rook and bishop movements
        for (int i = 0; i < 8; i++)
        {
            if (i != currentPos.x) moves.Add(new Vector2Int(i, currentPos.y));
            if (i != currentPos.y) moves.Add(new Vector2Int(currentPos.x, i));
        }

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