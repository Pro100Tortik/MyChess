using UnityEngine;
using System.Collections.Generic;

public class ChessGameManager : MonoBehaviour
{
    public static ChessGameManager Instance { get; private set; }
    public Vector2Int? EnPassantTarget { get; private set; }
    public Transform PieceParent => pieceParent;

    [Header("References")]
    [SerializeField] private AIPlayer ai;
    [SerializeField] private Transform pieceParent;
    [SerializeField] private BoardCreator boardCreator;
    [SerializeField] private PieceSpawner pieceSpawner;
    private bool gameOver = false;

    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
    public Piece SelectedPiece { get; private set; }
    public BoardCreator Board => boardCreator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeGame()
    {
        boardCreator.InitializeTiles();
        pieceSpawner.SpawnPieces();
    }

    public void InitializeGame(string FEN)
    {
        boardCreator.InitializeTiles();
        pieceSpawner.SpawnPieces(FEN);
    }

    public void SelectPiece(Piece piece)
    {
        if (piece.Data.Color != CurrentTurn) return;

        DeselectPiece();
        SelectedPiece = piece;
        HighlightPossibleMoves(piece);
    }

    public void DeselectPiece()
    {
        if (SelectedPiece != null)
        {
            SelectedPiece = null;
            ClearHighlights();
        }
    }

    public void OnTileClicked(Tile tile)
    {
        if (SelectedPiece == null)
        {
            // Выбор фигуры, если кликнули на свою
            if (tile.CurrentPiece != null && tile.CurrentPiece.Data.Color == CurrentTurn)
            {
                SelectPiece(tile.CurrentPiece);
            }
            return;
        }

        TryMakeMove(tile);
    }

    public void MakeMoveSimulated(Move move)
    {
        move.CapturedPiece = move.TargetTile.CurrentPiece;
        move.SourceTile = move.Piece.CurrentTile;

        move.TargetTile.CurrentPiece = move.Piece;
        move.SourceTile.CurrentPiece = null;

        move.Piece.Data.BoardPosition = move.TargetTile.BoardPosition;
    }

    public void UndoMoveSimulated(Move move)
    {
        move.TargetTile.CurrentPiece = move.CapturedPiece;
        move.SourceTile.CurrentPiece = move.Piece;

        move.Piece.Data.BoardPosition = move.SourceTile.BoardPosition;
    }

    public bool IsGameOver()
    {
        return !HasLegalMoves(CurrentTurn);
    }

    public void TryMakeMove(Tile targetTile)
    {
        if (SelectedPiece == null) return;

        if (IsValidMove(SelectedPiece, targetTile))
        {
            Debug.Log("Move is valid");
            MakeMove(SelectedPiece, targetTile);
        }
        else
        {
            Debug.Log("Invalid move");
            SelectedPiece.ResetPosition();
        }
        DeselectPiece();
    }

    public void MakeMove(Piece piece, Tile targetTile)
    {
        int direction = piece.Data.Color == PieceColor.White ? -1 : 1;

        // EnPassant
        if (piece.Data.Type == PieceType.Pawn &&
            targetTile.CurrentPiece == null &&
            EnPassantTarget.HasValue &&
            targetTile.BoardPosition == (new Vector2Int(EnPassantTarget.Value.x, EnPassantTarget.Value.y + direction * -1)))
        {
            Vector2Int capturedPawnPos = new Vector2Int(targetTile.BoardPosition.x, targetTile.BoardPosition.y + direction);
            Tile capturedTile = boardCreator.Tiles[capturedPawnPos.x, capturedPawnPos.y];

            if (capturedTile.CurrentPiece != null)
            {
                Destroy(capturedTile.CurrentPiece.gameObject);
                capturedTile.CurrentPiece = null;
            }
        }

        // Regular capture
        if (targetTile.CurrentPiece != null)
        {
            Destroy(targetTile.CurrentPiece.gameObject);
            targetTile.CurrentPiece = null;
        }

        if (piece.Data.Type == PieceType.King &&
            Mathf.Abs(targetTile.BoardPosition.x - piece.Data.BoardPosition.x) == 2)
        {
            direction = (targetTile.BoardPosition.x - piece.Data.BoardPosition.x) > 0 ? 1 : -1;
            int rookStartX = direction > 0 ? 7 : 0;
            int rookTargetX = direction > 0 ? 5 : 3;

            Tile rookStartTile = boardCreator.Tiles[rookStartX, piece.Data.BoardPosition.y];
            Tile rookTargetTile = boardCreator.Tiles[rookTargetX, piece.Data.BoardPosition.y];

            Piece rook = rookStartTile.CurrentPiece;
            if (rook != null && rook.Data.Type == PieceType.Rook)
            {
                rook.MoveTo(rookTargetTile);
            }
        }

        // Update EnPassant position
        if (piece.Data.Type == PieceType.Pawn)
        {
            int deltaY = Mathf.Abs(piece.CurrentTile.BoardPosition.y - targetTile.BoardPosition.y);
            if (deltaY == 2)
            {
                EnPassantTarget = targetTile.BoardPosition;
            }
            else
            {
                EnPassantTarget = null;
            }
        }
        else
        {
            EnPassantTarget = null;
        }

        piece.CurrentTile.CurrentPiece = null;
        piece.MoveTo(targetTile);
        targetTile.CurrentPiece = piece;

        CheckGameState();

        // Change turn
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

        // Make ai move
        if (CurrentTurn == PieceColor.Black)
        {
            ai.RequestAIMove();
        }
    }

    private void CheckGameState()
    {
        if (gameOver) return;

        bool isCheck = IsKingInCheck(CurrentTurn);
        bool hasLegalMoves = HasLegalMoves(CurrentTurn);

        if (isCheck && !hasLegalMoves)
        {
            Debug.Log($"Мат! Победил: + " + (CurrentTurn == PieceColor.White ? "Белый" : "Чёрный"));
            gameOver = true;
        }
        else if (!hasLegalMoves)
        {
            Debug.Log("Пат! Ничья");
            gameOver = true;
        }
        else if (isCheck)
        {
            Debug.Log("Шах!");
        }
    }

    public bool IsKingInCheck(PieceColor color)
    {
        Vector2Int kingPos = FindKingPosition(color);

        foreach (var tile in Board.Tiles)
        {
            if (tile.CurrentPiece != null && tile.CurrentPiece.Data.Color != color)
            {
                var moves = tile.CurrentPiece.GetPossibleMoves(false);
                if (moves.Contains(kingPos)) return true;
            }
        }
        return false;
    }

    public bool HasLegalMoves(PieceColor color)
    {
        // Проверяем все фигуры текущего игрока
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = Board.Tiles[x, y].CurrentPiece;
                if (piece != null && piece.Data.Color == color)
                {
                    List<Vector2Int> moves = piece.GetPossibleMoves();

                    // Фильтруем ходы, которые не оставляют короля под шахом
                    foreach (var move in moves)
                    {
                        // Симулируем ход
                        Tile targetTile = Board.Tiles[move.x, move.y];
                        Piece capturedPiece = targetTile.CurrentPiece;

                        targetTile.CurrentPiece = piece;
                        piece.CurrentTile.CurrentPiece = null;
                        Vector2Int originalPos = piece.Data.BoardPosition;
                        piece.Data.BoardPosition = move;

                        // Проверяем, остался ли король под шахом
                        bool isKingSafe = !IsKingInCheck(color);

                        // Отменяем симуляцию
                        piece.Data.BoardPosition = originalPos;
                        piece.CurrentTile.CurrentPiece = piece;
                        targetTile.CurrentPiece = capturedPiece;

                        if (isKingSafe)
                        {
                            return true; // Нашли хотя бы один допустимый ход
                        }
                    }
                }
            }
        }
        return false; // Нет допустимых ходов
    }

    public bool IsSquareUnderAttack(Vector2Int position, PieceColor defenderColor)
    {
        foreach (var tile in Board.Tiles)
        {
            Piece attacker = tile.CurrentPiece;
            if (attacker != null && attacker.Data.Color != defenderColor)
            {
                var attackerMoves = attacker.GetPossibleMoves(false);
                if (attackerMoves.Contains(position))
                    return true;
            }
        }
        return false;
    }

    public Vector2Int FindKingPosition(PieceColor color)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = Board.Tiles[x, y].CurrentPiece;
                if (piece != null &&
                    piece.Data.Type == PieceType.King &&
                    piece.Data.Color == color)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }

    private bool IsValidMove(Piece movingPiece, Tile targetTile)
    {
        if (movingPiece == null || targetTile == null) return false;
        if (movingPiece.Data.Color != CurrentTurn) return false;
        if (movingPiece.CurrentTile == targetTile) return false;

        var possibleMoves = movingPiece.GetPossibleMoves();
        if (!possibleMoves.Contains(targetTile.BoardPosition)) 
        {
            Debug.Log("No possible moves found");
            return false; 
        }

        if (targetTile.CurrentPiece != null &&
            targetTile.CurrentPiece.Data.Color == movingPiece.Data.Color)
            return false;

        return true;
    }

    private void HighlightPossibleMoves(Piece piece)
    {
        ClearHighlights();

        var moves = piece.GetPossibleMoves();
        foreach (var move in moves)
        {
            Tile tile = Board.Tiles[move.x, move.y];
            if (tile.CurrentPiece == null)
                tile.HighlightAsMove();
            else if (tile.CurrentPiece.Data.Color != piece.Data.Color)
                tile.HighlightAsDanger();
        }
    }

    private void ClearHighlights()
    {
        foreach (var tile in Board.Tiles)
        {
            tile?.ResetColor();
        }
    }
}