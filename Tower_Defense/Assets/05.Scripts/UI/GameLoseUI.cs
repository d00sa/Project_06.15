using UnityEngine;

public class GameLoseUI : MonoBehaviour
{
    public void Start()
    {
        SoundManager.Instance.StopAllSounds();
        SoundManager.Instance.PlaySFX("GameLose");
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
