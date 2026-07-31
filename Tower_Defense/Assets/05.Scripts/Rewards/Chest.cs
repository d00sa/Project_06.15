using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public void OnClick()
    {
        UIManager.Instance.ShowRewards();
        SoundManager.Instance.PlaySFX("Treasure_ChestSFX");
        ObjectPool.Instance.ReturnObj(this.gameObject);
    }
}
