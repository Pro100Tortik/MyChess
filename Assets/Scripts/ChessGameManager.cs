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

    private void MakeMove(Piece piece, Tile targetTile)
    {
        // Взятие фигуры
        if (targetTile.CurrentPiece != null)
        {
            Destroy(targetTile.CurrentPiece.gameObject);
        }

        // Обновляем позиции
        piece.CurrentTile.CurrentPiece = null;
        piece.MoveTo(targetTile);
        targetTile.CurrentPiece = piece;

        // Меняем очередь хода
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
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