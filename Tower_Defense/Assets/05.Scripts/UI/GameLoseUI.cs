using UnityEngine;

public class GameLoseUI : MonoBehaviour
{
    public void ReStart()
    {
        GameManager.Instance.GoToDifficultySelect();
    }

    public void GameQuit()
    {
        GameManager.Instance.GameQuit();
    }
}
