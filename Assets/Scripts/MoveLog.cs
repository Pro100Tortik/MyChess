[System.Serializable]
public class MoveLog
{
    public string FENKey;
    public string Description;

    public MoveLog(string fenKey, string description)
    {
        FENKey = fenKey;
        Description = description;
    }
}
