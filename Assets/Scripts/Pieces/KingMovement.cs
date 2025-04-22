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

        //if (checkKingSafety && !gameManager.IsKingInCheck(piece.Data.Color))
        //{
        //    AddCastlingMoves(moves, currentPos);
        //}

        return moves;
    }

    private void AddCastlingMoves(List<Vector2Int> moves, Vector2Int currentPos)
    {
        //bool canKingsideCastle = piece.Data.Color == PieceColor.White ?
        //    gameManager.WhiteCanKingsideCastle :
        //    gameManager.BlackCanKingsideCastle;

        //bool canQueensideCastle = piece.Data.Color == PieceColor.White ?
        //    gameManager.WhiteCanQueensideCastle :
        //    gameManager.BlackCanQueensideCastle;

        //if (canKingsideCastle && IsCastlingPathClear(1))
        //{
        //    moves.Add(new Vector2Int(currentPos.x + 2, currentPos.y));
        //}

        //if (canQueensideCastle && IsCastlingPathClear(-1))
        //{
        //    moves.Add(new Vector2Int(currentPos.x - 2, currentPos.y));
        //}
    }

    private bool IsCastlingPathClear(int direction)
    {
        Vector2Int currentPos = piece.Data.BoardPosition;
        int rookX = direction > 0 ? 7 : 0;

        Tile rookTile = gameManager.Board.Tiles[rookX, currentPos.y];
        if (rookTile.CurrentPiece == null ||
            rookTile.CurrentPiece.Data.Type != PieceType.Rook ||
            rookTile.CurrentPiece.Data.Color != piece.Data.Color)
        {
            return false;
        }

        int startX = direction > 0 ? currentPos.x + 1 : currentPos.x - 1;
        int endX = direction > 0 ? rookX - 1 : rookX + 1;

        for (int x = startX; x != endX; x += direction)
        {
            if (gameManager.Board.Tiles[x, currentPos.y].CurrentPiece != null)
                return false;
        }

        //for (int x = currentPos.x; x != currentPos.x + 2 * direction; x += direction)
        //{
        //    if (gameManager.IsSquareUnderAttack(
        //        new Vector2Int(x, currentPos.y), piece.Data.Color))
        //    {
        //        return false;
        //    }
        //}

        return true;
    }
}