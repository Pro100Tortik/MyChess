using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Piece : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public PieceData Data { get; private set; }
    public Tile CurrentTile { get; set; }

    private RectTransform _rectTransform;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Transform _originalParent;
    private Vector2 _startAnchoredPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _originalParent = transform.parent;
    }

    public void Initialize(PieceData data)
    {
        Data = data;
        GetComponent<Image>().color = Data.Color == PieceColor.White ? Color.white : Color.black;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;

        _canvasGroup.blocksRaycasts = false;

        transform.SetParent(ChessGameManager.Instance.PieceParent);
        transform.SetAsLastSibling();
        _startAnchoredPosition = _rectTransform.anchoredPosition;

        ChessGameManager.Instance.SelectPiece(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn)
        {
            Debug.Log("Not your turn");
            return;
        }

        _canvasGroup.blocksRaycasts = true;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent<Tile>(out var tile))
            {
                ChessGameManager.Instance.TryMakeMove(tile);
                //// Resets piece position to be inside tile
                //transform.localPosition = Vector2.zero;
                return;
            }
        }

        ResetPosition();
    }

    public void MoveTo(Tile targetTile)
    {
        Data.HasMoved = true;

        CurrentTile.CurrentPiece = null;
        CurrentTile = targetTile;

        transform.SetParent(ChessGameManager.Instance.PieceParent);
        transform.SetAsLastSibling();

        RectTransform tileRect = targetTile.GetComponent<RectTransform>();
        Vector3 worldPos = tileRect.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out Vector2 localPos
        );

        _rectTransform.anchoredPosition = localPos;

        Data.BoardPosition = targetTile.BoardPosition;
    }

    public void SetCurrentTile(Tile targetTile)
    {
        CurrentTile = targetTile;
    }

    public List<Vector2Int> GetPossibleMoves(bool checkKingSafety = true)
    {
        var movement = GetComponent<PieceMovement>();
        return movement?.GetPossibleMoves(checkKingSafety) ?? new List<Vector2Int>();
    }

    public void ResetPosition() => _rectTransform.anchoredPosition = _startAnchoredPosition;
}