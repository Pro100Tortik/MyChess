using UnityEngine;
using System.Collections.Generic;

public class ChessGameManager : MonoBehaviour
{
    public static ChessGameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private BoardCreator boardCreator;
    [SerializeField] private PieceSpawner pieceSpawner;

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

    private void Start() => InitializeGame();

    public void InitializeGame()
    {
        boardCreator.InitializeTiles();
        pieceSpawner.SpawnPieces();
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

    public void TryMakeMove(Tile targetTile)
    {
        if (SelectedPiece == null) return;

        if (IsValidMove(SelectedPiece, targetTile))
        {
            MakeMove(SelectedPiece, targetTile);
        }
        else
        {
            SelectedPiece.ResetPosition();
        }
        DeselectPiece();
    }

    public Vector2Int? EnPassantTarget { get; private set; }

    private void MakeMove(Piece piece, Tile targetTile)
    {
        // Удаление фигуры противника
        if (targetTile.CurrentPiece != null)
        {
            Destroy(targetTile.CurrentPiece.gameObject);
        }

        // Перемещение без рекурсии
        piece.CurrentTile.CurrentPiece = null;
        piece.MoveTo(targetTile);  // MoveTo не должен вызывать TryMakeMove
        targetTile.CurrentPiece = piece;

        // Смена хода
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private void CheckGameState()
    {
        bool isCheck = IsKingInCheck(CurrentTurn);
        bool hasLegalMoves = HasLegalMoves(CurrentTurn);

        if (isCheck && !hasLegalMoves)
        {

        }
        else if (!hasLegalMoves)
        {
            Debug.Log("Пат! Ничья");
        }
        else if (isCheck)
        {
            Debug.Log("Шах!");
        }
    }

    public bool IsKingInCheck(PieceColor color)
    {
        // Найти короля
        Vector2Int kingPos = FindKingPosition(color);

        // Проверить, атакован ли он
        foreach (var tile in Board.Tiles)
        {
            if (tile.CurrentPiece != null && tile.CurrentPiece.Data.Color != color)
            {
                var moves = tile.CurrentPiece.GetPossibleMoves();
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

    private bool IsValidMove(Piece piece, Tile targetTile)
    {
        // Базовые проверки
        if (piece == null || targetTile == null) return false;
        if (piece.Data.Color != CurrentTurn) return false;
        if (piece.CurrentTile == targetTile) return false;

        // Проверка правил движения
        var possibleMoves = piece.GetPossibleMoves();
        if (!possibleMoves.Contains(targetTile.BoardPosition)) return false;

        // Проверка на свои фигуры
        if (targetTile.CurrentPiece != null &&
            targetTile.CurrentPiece.Data.Color == piece.Data.Color)
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