using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public ChessGameManager gameManager;
    public float maxThinkTime = 2.8f;
    private float startTime;

    public void RequestAIMove()
    {
        StartCoroutine(ComputeBestMoveAsync());
    }

    private IEnumerator ComputeBestMoveAsync()
    {
        yield return null;

        Move bestMove = null;
        float bestEval = float.NegativeInfinity;
        PieceColor aiColor = gameManager.CurrentTurn;

        List<Move> moves = GenerateAllPseudoLegalMoves(aiColor);
        moves.Sort((a, b) => GetMoveScore(b).CompareTo(GetMoveScore(a)));

        int depth = moves.Count > 20 ? 2 : 3;
        startTime = Time.realtimeSinceStartup;

        foreach (Move move in moves)
        {
            MakeMoveSimulated(move);
            if (!gameManager.IsKingInCheck(aiColor)) // фильтр мата себе
            {
                float eval = Minimax(depth - 1, float.NegativeInfinity, float.PositiveInfinity, false);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }
            UndoMoveSimulated(move);

            if (Time.realtimeSinceStartup - startTime > maxThinkTime)
                break;
        }

        if (bestMove != null)
            gameManager.MakeMove(bestMove.Piece, bestMove.TargetTile);
    }

    private float Minimax(int depth, float alpha, float beta, bool maximizing)
    {
        if (depth == 0 || gameManager.IsGameOver() || Time.realtimeSinceStartup - startTime > maxThinkTime)
            return EvaluateBoard();

        PieceColor color = maximizing ? gameManager.CurrentTurn : OpponentColor(gameManager.CurrentTurn);
        List<Move> moves = GenerateAllPseudoLegalMoves(color);
        moves.Sort((a, b) => GetMoveScore(b).CompareTo(GetMoveScore(a)));

        if (maximizing)
        {
            float maxEval = float.NegativeInfinity;
            foreach (var move in moves)
            {
                MakeMoveSimulated(move);
                if (!gameManager.IsKingInCheck(color))
                {
                    float eval = Minimax(depth - 1, alpha, beta, false);
                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);
                }
                UndoMoveSimulated(move);
                if (beta <= alpha || Time.realtimeSinceStartup - startTime > maxThinkTime)
                    break;
            }
            return maxEval;
        }
        else
        {
            float minEval = float.PositiveInfinity;
            foreach (var move in moves)
            {
                MakeMoveSimulated(move);
                if (!gameManager.IsKingInCheck(color))
                {
                    float eval = Minimax(depth - 1, alpha, beta, true);
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                }
                UndoMoveSimulated(move);
                if (beta <= alpha || Time.realtimeSinceStartup - startTime > maxThinkTime)
                    break;
            }
            return minEval;
        }
    }

    private float EvaluateBoard()
    {
        float total = 0;
        foreach (var tile in gameManager.Board.Tiles)
        {
            if (tile.CurrentPiece == null) continue;
            float value = GetPieceValue(tile.CurrentPiece.Data.Type);
            total += tile.CurrentPiece.Data.Color == PieceColor.White ? value : -value;
        }
        return total;
    }

    private float GetPieceValue(PieceType type) => type switch
    {
        PieceType.Pawn => 1f,
        PieceType.Knight => 3f,
        PieceType.Bishop => 3f,
        PieceType.Rook => 5f,
        PieceType.Queen => 9f,
        PieceType.King => 100f,
        _ => 0f,
    };

    private int GetMoveScore(Move move)
    {
        if (move.TargetTile.CurrentPiece != null)
            return (int)GetPieceValue(move.TargetTile.CurrentPiece.Data.Type) * 10 -
                   (int)GetPieceValue(move.Piece.Data.Type);
        return 0;
    }

    private List<Move> GenerateAllPseudoLegalMoves(PieceColor color)
    {
        var moves = new List<Move>();
        foreach (var tile in gameManager.Board.Tiles)
        {
            Piece piece = tile.CurrentPiece;
            if (piece != null && piece.Data.Color == color)
            {
                foreach (var movePos in piece.GetPossibleMoves(false)) // псевдо-легальные
                {
                    Tile target = gameManager.Board.Tiles[movePos.x, movePos.y];
                    moves.Add(new Move(piece, target));
                }
            }
        }
        return moves;
    }

    private PieceColor OpponentColor(PieceColor color)
    {
        return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private void MakeMoveSimulated(Move move)
    {
        move.SourceTile = move.Piece.CurrentTile;
        move.CapturedPiece = move.TargetTile.CurrentPiece;

        move.TargetTile.CurrentPiece = move.Piece;
        move.SourceTile.CurrentPiece = null;

        move.Piece.Data.BoardPosition = move.TargetTile.BoardPosition;
        move.Piece.CurrentTile = move.TargetTile;
    }

    private void UndoMoveSimulated(Move move)
    {
        move.SourceTile.CurrentPiece = move.Piece;
        move.TargetTile.CurrentPiece = move.CapturedPiece;

        move.Piece.Data.BoardPosition = move.SourceTile.BoardPosition;
        move.Piece.CurrentTile = move.SourceTile;
    }
}
