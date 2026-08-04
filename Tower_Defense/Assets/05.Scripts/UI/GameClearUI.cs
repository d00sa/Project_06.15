using UnityEngine;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] RectTransform _panels;
    private void Start()
    {
        SoundManager.Instance.StopAllSounds();

        _panels.localScale = Vector3.one;
        SoundManager.Instance.PlaySFX("GameVictory");
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
