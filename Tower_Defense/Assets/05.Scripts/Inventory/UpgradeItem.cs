using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItem : MonoBehaviour
{
    Button _upgradeButton;

    private void Awake()
    {
        _upgradeButton = GetComponent<Button>();
    }

    private void Start()
    {
        Setting();
    }

    private void Setting()
    {
        _upgradeButton.onClick.AddListener(() =>
        {
            //Todo: 아이템 업그레이드 + 풀 업시 버튼 비활성화            
        });
    }    
}
