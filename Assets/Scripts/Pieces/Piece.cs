using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class Piece : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public PieceData Data { get; private set; }
    public Tile CurrentTile { get; set; }
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector2 _startPosition;
    private Transform _originalParent;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _originalParent = transform.parent;
    }

    public void Initialize(PieceData data) => Data = data;

    // Начало перетаскивания
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;

        _startPosition = _rectTransform.anchoredPosition;
        _canvasGroup.blocksRaycasts = false; // Чтобы видеть клетки под фигурой
        transform.SetParent(_canvas.transform); // Поднимаем фигуру над всеми UI-элементами
        ChessGameManager.Instance.SelectPiece(this);
    }

    // Процесс перетаскивания
    public void OnDrag(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    // Завершение перетаскивания
    public void OnPointerUp(PointerEventData eventData)
    {
        if (Data.Color != ChessGameManager.Instance.CurrentTurn) return;

        _canvasGroup.blocksRaycasts = true;
        transform.SetParent(_originalParent); // Возвращаем фигуру в исходный parent

        // Проверяем, была ли фигура отпущена над клеткой
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent<Tile>(out var tile))
            {
                ChessGameManager.Instance.TryMakeMove(tile);
                return;
            }
        }

        ResetPosition(); // Если не попали на клетку
    }

    public void MoveTo(Tile targetTile)
    {
        // Убедитесь, что этот метод не вызывает ChessGameManager.Instance.TryMakeMove
        CurrentTile.CurrentPiece = null;
        CurrentTile = targetTile;
        targetTile.CurrentPiece = this;
        _rectTransform.anchoredPosition = targetTile.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetCurrentTile(Tile targetTile)
    {
        CurrentTile = targetTile;
    }

    public List<Vector2Int> GetPossibleMoves()
    {
        var movement = GetComponent<PieceMovement>();
        return movement?.GetPossibleMoves() ?? new List<Vector2Int>();
    }

    public void ResetPosition() => _rectTransform.anchoredPosition = _startPosition;
}