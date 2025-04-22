using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PieceData Data { get; private set; }
    public Tile CurrentTile { get; set; }

    private RectTransform _rectTransform;
    private Vector2 _startPosition;
    private Camera _mainCamera;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    public void Initialize(PieceData data)
    {
        Data = data;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;

        _startPosition = _rectTransform.anchoredPosition;
        ChessGameManager.Instance.SelectPiece(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;

        var hit = Physics2D.Raycast(
            _mainCamera.ScreenToWorldPoint(Input.mousePosition),
            Vector2.zero);

        if (hit.collider != null && hit.collider.TryGetComponent<Tile>(out var tile))
        {
            ChessGameManager.Instance.TryMakeMove(tile);
        }
        else
        {
            ResetPosition();
        }
    }

    public void MoveTo(Tile targetTile)
    {
        CurrentTile = targetTile;
        _rectTransform.SetParent(targetTile.transform);
        _rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetCurrentTile(Tile tile)
    {
        MoveTo(tile);
    }

    public void ResetPosition()
    {
        _rectTransform.anchoredPosition = _startPosition;
    }

    public List<Vector2Int> GetPossibleMoves()
    {
        var movement = GetComponent<PieceMovement>();
        return movement?.GetPossibleMoves() ?? new List<Vector2Int>();
    }
}