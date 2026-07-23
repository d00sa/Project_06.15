using UnityEngine;

public abstract class ConsumableEffect : ScriptableObject
{
    // 아이템 소모시 실행
    public abstract void Execute();
}
