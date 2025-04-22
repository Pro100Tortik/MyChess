using System.Collections.Generic;
using UnityEngine;

public static class FENParser
{
    #region Read
    public static List<PieceData> ParseFEN(string fen)
    {
        var pieces = new List<PieceData>();
        string[] parts = fen.Split(' ');
        string[] rows = parts[0].Split('/');

        for (int y = 0; y < 8; y++)
        {
            string row = rows[y];
            int x = 0;

            foreach (char c in row)
            {
                if (char.IsDigit(c))
                {
                    x += c - '0';
                }
                else
                {
                    var piece = new PieceData
                    {
                        Color = char.IsUpper(c) ? PieceColor.White : PieceColor.Black,
                        Type = CharToType(char.ToLower(c)),
                        BoardPosition = new Vector2Int(x, 7 - y)
                    };
                    pieces.Add(piece);
                    x++;
                }
            }
        }

        return pieces;
    }

    private static PieceType CharToType(char c) => c switch
    {
        'p' => PieceType.Pawn,
        'r' => PieceType.Rook,
        'n' => PieceType.Knight,
        'b' => PieceType.Bishop,
        'q' => PieceType.Queen,
        'k' => PieceType.King,
        _ => default
    };
    #endregion

    #region Write
    public static string GenerateFEN(List<PieceData> pieces)
    {
        PieceData[,] board = new PieceData[8, 8];

        foreach (var piece in pieces)
        {
            board[piece.BoardPosition.x, piece.BoardPosition.y] = piece;
        }

        List<string> ranks = new();

        for (int y = 7; y >= 0; y--)
        {
            string rank = "";
            int emptyCount = 0;

            for (int x = 0; x < 8; x++)
            {
                var piece = board[x, y];
                if (piece == null)
                {
                    emptyCount++;
                    continue;
                }

                if (emptyCount > 0)
                {
                    rank += emptyCount.ToString();
                    emptyCount = 0;
                }

                rank += TypeToChar(piece.Type, piece.Color);
            }

            if (emptyCount > 0)
                rank += emptyCount.ToString();

            ranks.Add(rank);
        }

        return string.Join("/", ranks);
    }

    private static char TypeToChar(PieceType type, PieceColor color)
    {
        char c = type switch
        {
            PieceType.Pawn => 'p',
            PieceType.Rook => 'r',
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b',
            PieceType.Queen => 'q',
            PieceType.King => 'k',
            _ => '?'
        };

        return color == PieceColor.White ? char.ToUpper(c) : c;
    }
    #endregion
}