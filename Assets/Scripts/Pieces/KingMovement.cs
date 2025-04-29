using System.Collections.Generic;
using UnityEngine;

public class KingMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int currentPos = piece.Data.BoardPosition;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                Vector2Int move = new Vector2Int(currentPos.x + x, currentPos.y + y);
                if (IsMoveValid(move))
                    moves.Add(move);
            }
        }

        if (checkKingSafety &&
            !ChessGameManager.Instance.IsKingInCheck(piece.Data.Color))
        {
            AddCastlingMoves(moves, currentPos);
        }

        return checkKingSafety ? FilterValidMoves(moves) : moves;
    }

    private void AddCastlingMoves(List<Vector2Int> moves, Vector2Int currentPos)
    {
        if (!piece.Data.HasMoved)
        {
            TryAddCastlingMove(moves, currentPos, kingside: true);
            TryAddCastlingMove(moves, currentPos, kingside: false);
        }
    }

    private void TryAddCastlingMove(List<Vector2Int> moves, Vector2Int kingPos, bool kingside)
    {
        int rookX = kingside ? 7 : 0;
        int direction = kingside ? 1 : -1;

        Tile rookTile = gameManager.Board.Tiles[rookX, kingPos.y];
        Piece rook = rookTile.CurrentPiece;

        if (rook != null &&
            rook.Data.Type == PieceType.Rook &&
            rook.Data.Color == piece.Data.Color &&
            !rook.Data.HasMoved &&
            IsCastlingPathClear(kingPos, rookX, direction) &&
            AreCastlingSquaresSafe(kingPos, direction))
        {
            int kingTargetX = kingside ? 6 : 2;
            moves.Add(new Vector2Int(kingTargetX, kingPos.y));
        }
    }

    private bool IsCastlingPathClear(Vector2Int kingPos, int rookX, int direction)
    {
        int startX = kingPos.x + direction;
        int endX = rookX;

        for (int x = startX; x != endX; x += direction)
        {
            if (gameManager.Board.Tiles[x, kingPos.y].CurrentPiece != null)
                return false;
        }

        return true;
    }

    private bool AreCastlingSquaresSafe(Vector2Int kingPos, int direction)
    {
        for (int i = 0; i <= 2; i++)
        {
            int x = kingPos.x + i * direction;
            if (gameManager.IsSquareUnderAttack(new Vector2Int(x, kingPos.y), piece.Data.Color))
                return false;
        }
        return true;
    }
}