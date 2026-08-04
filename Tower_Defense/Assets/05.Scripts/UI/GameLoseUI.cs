using UnityEngine;

public class GameLoseUI : MonoBehaviour
{
    [SerializeField] RectTransform _panels;
    public void Start()
    {
        SoundManager.Instance.StopAllSounds();

        _panels.localScale = Vector3.one;
        SoundManager.Instance.PlaySFX("GameLose");
    }
    public void ReStart()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");

        _panels.localScale = Vector3.zero;
        GameManager.Instance.GoToDifficultySelect();
    }

    public void GameQuit()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.GameQuit();
    }
}
