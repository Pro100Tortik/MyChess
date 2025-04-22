using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    [Header("Settings")]
    [SerializeField] private List<PiecePrefab> piecePrefabs = new List<PiecePrefab>();
    [SerializeField] private string initialFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    [Header("References")]
    [SerializeField] private BoardCreator boardCreator;

    private Dictionary<PieceType, GameObject> _prefabDict = new Dictionary<PieceType, GameObject>();
    private List<Piece> _spawnedPieces = new List<Piece>();

    private void Awake()
    {
        foreach (var piecePrefab in piecePrefabs)
        {
            _prefabDict[piecePrefab.type] = piecePrefab.prefab;
        }
    }

    public void SpawnPieces()
    {
        ClearPieces();
        var pieceDataList = FENParser.ParseFEN(initialFEN);

        foreach (var pieceData in pieceDataList)
        {
            SpawnPiece(pieceData);
        }
    }

    private void SpawnPiece(PieceData data)
    {
        if (!_prefabDict.ContainsKey(data.Type))
        {
            Debug.LogError($"No prefab for piece type: {data.Type}");
            return;
        }

        Tile targetTile = boardCreator.Tiles[data.BoardPosition.x, data.BoardPosition.y];
        if (targetTile == null)
        {
            Debug.LogError($"No tile at position: {data.BoardPosition}");
            return;
        }

        GameObject pieceGO = Instantiate(_prefabDict[data.Type], targetTile.transform);
        pieceGO.transform.localPosition = Vector3.zero;

        Piece piece = pieceGO.GetComponent<Piece>();
        piece.Initialize(data);

        piece.SetCurrentTile(targetTile);

        AddMovementComponent(pieceGO, data.Type);

        targetTile.CurrentPiece = piece;
        _spawnedPieces.Add(piece);
    }

    private void AddMovementComponent(GameObject pieceGO, PieceType type)
    {
        var existingMovement = pieceGO.GetComponent<PieceMovement>();
        if (existingMovement != null)
        {
            Destroy(existingMovement);
        }

        switch (type)
        {
            case PieceType.Pawn:
                pieceGO.AddComponent<PawnMovement>();
                break;
            case PieceType.Rook:
                pieceGO.AddComponent<RookMovement>();
                break;
            case PieceType.Knight:
                pieceGO.AddComponent<KnightMovement>();
                break;
            case PieceType.Bishop:
                pieceGO.AddComponent<BishopMovement>();
                break;
            case PieceType.Queen:
                pieceGO.AddComponent<QueenMovement>();
                break;
            case PieceType.King:
                pieceGO.AddComponent<KingMovement>();
                break;
        }
    }

    public void ClearPieces()
    {
        foreach (var piece in _spawnedPieces)
        {
            if (piece != null && piece.gameObject != null)
            {
                Destroy(piece.gameObject);
            }
        }
        _spawnedPieces.Clear();
    }

    [ContextMenu("Validate Piece Prefabs")]
    private void ValidatePiecePrefabs()
    {
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            bool found = false;
            foreach (var piecePrefab in piecePrefabs)
            {
                if (piecePrefab.type == type)
                {
                    found = true;
                    if (piecePrefab.prefab == null)
                    {
                        Debug.LogError($"Prefab for {type} is not assigned!");
                    }
                    else if (piecePrefab.prefab.GetComponent<Piece>() == null)
                    {
                        Debug.LogError($"Prefab for {type} is missing Piece component!");
                    }
                    break;
                }
            }

            if (!found)
            {
                Debug.LogError($"No prefab assigned for piece type: {type}");
            }
        }
    }
}