public class Move
{
    public Piece Piece;
    public Tile TargetTile;
    public Tile SourceTile;
    public Piece CapturedPiece;

    public Move(Piece piece, Tile target)
    {
        Piece = piece;
        TargetTile = target;
    }
}
