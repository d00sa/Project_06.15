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

        // 획득 효과음 재생

        // 플로팅 텍스트 띄우기: 타워 꼭대기에 "리롤 횟수 +1" 표시 이런 느낌?

        ObjectPool.Instance.ReturnObj(this.gameObject);
    }
}
