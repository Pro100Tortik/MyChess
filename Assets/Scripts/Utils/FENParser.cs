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

    public static bool IsValidFEN(string fen)
    {
        if (string.IsNullOrWhiteSpace(fen)) return false;

        string[] parts = fen.Split(' ');
        if (parts.Length != 6)
        {
            Debug.Log("FEN должен содержать 6 частей");
            return false;
        }

        // 1. ѕроверка расстановки фигур
        string[] rows = parts[0].Split('/');
        if (rows.Length != 8)
        {
            Debug.Log("FEN: должно быть 8 строк доски");
            return false;
        }

        foreach (string row in rows)
        {
            int count = 0;
            foreach (char c in row)
            {
                if (char.IsDigit(c))
                {
                    count += c - '0';
                }
                else if ("rnbqkpRNBQKP".Contains(c))
                {
                    count += 1;
                }
                else
                {
                    Debug.Log($"FEN: недопустимый символ в расстановке фигур: {c}");
                    return false;
                }
            }
            if (count != 8)
            {
                Debug.Log("FEN: сумма символов в строке доски должна быть 8");
                return false;
            }
        }

        // 2. ќчерЄдность хода
        if (parts[1] != "w" && parts[1] != "b")
        {
            Debug.Log("FEN: поле хода должно быть 'w' или 'b'");
            return false;
        }

        // 3. –окировка (допускаютс€ KQkq или -)
        if (!System.Text.RegularExpressions.Regex.IsMatch(parts[2], @"^(-|[KQkq]+)$"))
        {
            Debug.Log("FEN: поле рокировки недопустимо");
            return false;
        }

        // 4. ¬з€тие на проходе: либо '-', либо клетка (например, 'e3')
        if (parts[3] != "-" &&
            !System.Text.RegularExpressions.Regex.IsMatch(parts[3], @"^[a-h][36]$")) // только 3 или 6 р€д
        {
            Debug.Log("FEN: неверна€ клетка вз€ти€ на проходе");
            return false;
        }

        // 5. ѕолуходы (целое число)
        if (!int.TryParse(parts[4], out _))
        {
            Debug.Log("FEN: счЄтчик полуходов должен быть числом");
            return false;
        }

        // 6. ѕолный ход (целое число >= 1)
        if (!int.TryParse(parts[5], out int fullMove) || fullMove < 1)
        {
            Debug.Log("FEN: номер полного хода должен быть >= 1");
            return false;
        }

        return true;
    }
}