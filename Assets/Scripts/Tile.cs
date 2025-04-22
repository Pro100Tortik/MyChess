using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2Int BoardPosition { get; private set; }
    public Piece CurrentPiece { get; set; }

    [SerializeField] private Image tileImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color moveColor = Color.green;
    [SerializeField] private Color dangerColor = Color.red;

    public void Initialize(Vector2Int position, Color color)
    {
        BoardPosition = position;
        defaultColor = color;
        ResetColor();
    }

    public void HighlightAsMove() => tileImage.color = moveColor;
    public void HighlightAsDanger() => tileImage.color = dangerColor;
    public void ResetColor() => tileImage.color = defaultColor;
}