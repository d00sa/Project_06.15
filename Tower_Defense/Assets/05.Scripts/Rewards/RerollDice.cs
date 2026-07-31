using UnityEngine;

public class RerollDice : MonoBehaviour, IInteractable
{
    [Header("[리롤 횟수 추가량]")]
    [SerializeField] private int _addCount = 1;

    public void OnClick()
    {

        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.AddRerollCount(_addCount);
        }

        SoundManager.Instance.PlaySFX("DiceSFX");

        if (Player.Instance != null && DamageTextManager.Instance != null)
        {
            Vector2 textPos = Player.Instance.transform.position;

            DamageTextManager.Instance.ShowMessage($"리롤 횟수 +{_addCount}", textPos);
        }

        ObjectPool.Instance.ReturnObj(this.gameObject);
    }
}
