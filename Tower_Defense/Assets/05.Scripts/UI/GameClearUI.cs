using UnityEngine;

public class GameClearUI : MonoBehaviour
{
    private void Start()
    {
        SoundManager.Instance.PlaySFX("GameVictory");
    }
    public void ReStart()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.GoToDifficultySelect();
    }

    public void GameQuit()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.GameQuit();
    }
}
