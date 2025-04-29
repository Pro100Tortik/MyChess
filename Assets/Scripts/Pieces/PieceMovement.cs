using System.Collections.Generic;
using UnityEngine;

public abstract class PieceMovement : MonoBehaviour
{
    protected Piece piece;
    protected ChessGameManager gameManager;

    private void Awake()
    {
        piece = GetComponent<Piece>();
        gameManager = ChessGameManager.Instance;
    }

    public abstract List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true);

    protected bool IsMoveValid(Vector2Int targetPos)
    {
        if (targetPos.x < 0 || targetPos.x >= 8 || targetPos.y < 0 || targetPos.y >= 8)
        {
            Debug.Log("Out of bounds");
            return false;
        }

        Tile targetTile = gameManager.Board.Tiles[targetPos.x, targetPos.y];
        if (targetTile.CurrentPiece != null &&
            targetTile.CurrentPiece.Data.Color == piece.Data.Color)
        {
            Debug.Log("Same color");
            return false;
        }

        return true;
    }

    protected List<Vector2Int> FilterValidMoves(List<Vector2Int> rawMoves)
    {
        return rawMoves.FindAll(IsMoveValid);
    }

    protected List<Vector2Int> FilterBlockedMoves(List<Vector2Int> moves)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int currentPos = piece.Data.BoardPosition;

        foreach (var move in moves)
        {
            Vector2Int dir = move - currentPos;
            dir = new Vector2Int(
                dir.x != 0 ? dir.x / Mathf.Abs(dir.x) : 0,
                dir.y != 0 ? dir.y / Mathf.Abs(dir.y) : 0
            );

            bool pathClear = true;
            Vector2Int checkPos = currentPos + dir;

            while (checkPos != move && pathClear)
            {
                if (checkPos.x < 0 || checkPos.x >= 8 || checkPos.y < 0 || checkPos.y >= 8)
                {
                    pathClear = false;
                    break;
                }

                Tile tile = gameManager.Board.Tiles[checkPos.x, checkPos.y];
                if (tile.CurrentPiece != null)
                {
                    pathClear = false;
                    break;
                }

                checkPos += dir;
            }

            if (pathClear)
            {
                result.Add(move);
            }
        }

        return result;
    }

    protected List<Vector2Int> FilterMovesForKingSafety(List<Vector2Int> moves)
    {
        List<Vector2Int> safeMoves = new List<Vector2Int>();
        foreach (var move in moves)
        {
            // Симулируем ход
            Piece capturedPiece = null;
            Tile targetTile = gameManager.Board.Tiles[move.x, move.y];
            if (targetTile.CurrentPiece != null)
            {
                capturedPiece = targetTile.CurrentPiece;
                targetTile.CurrentPiece = null;
            }

            Vector2Int originalPos = piece.Data.BoardPosition;
            piece.Data.BoardPosition = move;

            // Проверяем, остался ли король под шахом
            bool isKingSafe = !gameManager.IsKingInCheck(piece.Data.Color);
            if (isKingSafe) safeMoves.Add(move);

            // Отменяем симуляцию
            piece.Data.BoardPosition = originalPos;
            if (capturedPiece != null)
            {
                targetTile.CurrentPiece = capturedPiece;
            }
        }
        return safeMoves;
    }
}