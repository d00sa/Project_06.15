using UnityEngine;

public class GameClearUI : MonoBehaviour
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
