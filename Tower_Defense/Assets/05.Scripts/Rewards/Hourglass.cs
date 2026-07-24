using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hourglass : MonoBehaviour, IInteractable
{
    [Header("[모래시계 설정]")]
    [SerializeField] private float _freezeDuration = 5f;

    public void OnClick()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartCoroutine(TimeStopRoutine());
        }

        ObjectPool.Instance.ReturnObj(this.gameObject);
    }

    private IEnumerator TimeStopRoutine()
    {
        GameManager.Instance.IsTimeStopped = true;

        yield return new WaitForSeconds(_freezeDuration);

        GameManager.Instance.IsTimeStopped = false;


    }
}
