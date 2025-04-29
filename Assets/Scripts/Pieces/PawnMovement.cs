using System.Collections.Generic;
using UnityEngine;

public class PawnMovement : PieceMovement
{
    public override List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        int direction = piece.Data.Color == PieceColor.White ? 1 : -1;
        Vector2Int currentPos = piece.Data.BoardPosition;

        Vector2Int moveForward = new Vector2Int(currentPos.x, currentPos.y + direction);
        if (IsMoveValid(moveForward) &&
            gameManager.Board.Tiles[moveForward.x, moveForward.y].CurrentPiece == null)
        {
            moves.Add(moveForward);

            if ((currentPos.y == 1 && piece.Data.Color == PieceColor.White) ||
                (currentPos.y == 6 && piece.Data.Color == PieceColor.Black))
            {
                Vector2Int doubleMove = new Vector2Int(currentPos.x, currentPos.y + 2 * direction);
                Tile doubleMoveTile = gameManager.Board.Tiles[doubleMove.x, doubleMove.y];
                if (doubleMoveTile.CurrentPiece == null)
                    moves.Add(doubleMove);
            }
        }

        CheckCapture(moves, currentPos.x + 1, currentPos.y + direction);
        CheckCapture(moves, currentPos.x - 1, currentPos.y + direction);

        CheckEnPassant(moves, currentPos.x + 1, currentPos.y);
        CheckEnPassant(moves, currentPos.x - 1, currentPos.y);

        return moves;
    }

    private void CheckCapture(List<Vector2Int> moves, int x, int y)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            Vector2Int capturePos = new Vector2Int(x, y);
            Tile captureTile = gameManager.Board.Tiles[x, y];
            if (captureTile.CurrentPiece != null &&
                captureTile.CurrentPiece.Data.Color != piece.Data.Color)
            {
                moves.Add(capturePos);
            }
        }
    }

    private void CheckEnPassant(List<Vector2Int> moves, int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return;

        Tile adjacentTile = gameManager.Board.Tiles[x, y];
        Piece adjacentPiece = adjacentTile.CurrentPiece;

        if (adjacentPiece != null &&
            adjacentPiece.Data.Type == PieceType.Pawn &&
            adjacentPiece.Data.Color != piece.Data.Color &&
            ChessGameManager.Instance.EnPassantTarget.HasValue &&
            ChessGameManager.Instance.EnPassantTarget.Value == new Vector2Int(x, y))
        {
            int direction = piece.Data.Color == PieceColor.White ? 1 : -1;
            Vector2Int enPassantMove = new Vector2Int(x, y + direction);

            if (IsMoveValid(enPassantMove))
            {
                moves.Add(enPassantMove);
            }
        }
    }
}