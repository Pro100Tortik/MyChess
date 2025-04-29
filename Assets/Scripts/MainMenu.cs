using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text fen;

    public void StartGameWithFEN()
    {
        if (FENParser.IsValidFEN(fen.text))
            ChessGameManager.Instance.InitializeGame(fen.text);
    }

    public void StartStandartGame()
    {
        ChessGameManager.Instance.InitializeGame();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
