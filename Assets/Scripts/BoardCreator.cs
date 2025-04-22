using UnityEngine;
using UnityEngine.UI;

public class BoardCreator : MonoBehaviour
{
    public Tile[,] Tiles { get; private set; } = new Tile[8, 8];

    [SerializeField] private Transform boardParent;
    [SerializeField] private GridLayoutGroup boardGrid;

    [SerializeField] private Color lightColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private Color darkColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [ContextMenu("Update visuals")]
    public void InitializeTiles()
    {
        Tiles = new Tile[8, 8];

        Tile[] allTiles = boardParent.GetComponentsInChildren<Tile>();

        foreach (Tile tile in allTiles)
        {
            string name = tile.gameObject.name;
            int x = name[name.Length - 2] - 'A';
            int y = name[name.Length - 1] - '1';

            tile.Initialize(new Vector2Int(x, y), (x + y) % 2 == 0 ? lightColor : darkColor);
            Tiles[x, y] = tile;
        }
    }
}
