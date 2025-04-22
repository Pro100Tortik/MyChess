using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class Tile : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int BoardPosition { get; private set; }
    public Piece CurrentPiece { get; set; }

    [SerializeField] private Image _tileImage;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _moveColor = Color.green;
    [SerializeField] private Color _dangerColor = Color.red;

    public void Initialize(Vector2Int position, Color color)
    {
        BoardPosition = position;
        _defaultColor = color;
        ResetColor();
    }

    // Обработка клика на клетку
    public void OnPointerClick(PointerEventData eventData)
    {
        ChessGameManager.Instance.OnTileClicked(this);
    }

    public void HighlightAsMove() => _tileImage.color = _moveColor;
    public void HighlightAsDanger() => _tileImage.color = _dangerColor;
    public void ResetColor() => _tileImage.color = _defaultColor;
}