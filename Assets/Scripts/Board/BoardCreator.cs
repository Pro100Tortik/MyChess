using UnityEngine;
using UnityEngine.UI;

public class BoardCreator : MonoBehaviour
{
    public Tile[,] Tiles { get; private set; } = new Tile[8, 8];

    [SerializeField] private Transform boardParent;
    [SerializeField] private GridLayoutGroup boardGrid;
    [SerializeField] private Tile tilePrefab;

    [SerializeField] private Color lightColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private Color darkColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [ContextMenu("Update visuals")]
    public void InitializeTiles()
    {
        Tiles = new Tile[8, 8];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Tile tile = Instantiate(tilePrefab, boardParent);
                tile.Initialize(new Vector2Int(x, y), (x + y) % 2 == 0 ? lightColor : darkColor);
                Tiles[x, y] = tile;
            }
        }
    }
}
