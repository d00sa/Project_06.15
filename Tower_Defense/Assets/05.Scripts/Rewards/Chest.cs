using UnityEngine;

public class Chest : MonoBehaviour
{
    public void OnClick()
    {
        UIManager.Instance.ShowRewards();
        //효과음도 재생하면 좋을 듯 여기서.
        //SoundManager.Instance.PlaySFX("Treasure_Chest");
        ObjectPool.Instance.ReturnObj(this.gameObject);
    }
}
