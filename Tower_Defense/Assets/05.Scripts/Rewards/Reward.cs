using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Reward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("[보상창 초기 설정]")]
    [SerializeField] private RectTransform _pos;
    [SerializeField] private ItemData _item;
    [SerializeField] private Image _icon; //장비 아이콘
    [SerializeField] private TMP_Text _name; //장비 이름 텍스트
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Image _frameImage;
    [SerializeField] private Button _button;

    Coroutine _curCoroutine;
    private void Awake()
    {
        _pos = GetComponent<RectTransform>();
        _frameImage = GetComponent<Image>();
    }

    public void SetRewards(ItemData item)
    {
        if (item == null) {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _name.text = "";
            _button.interactable = false;
            _description.text = "";
            _frameImage.sprite = UIManager.Instance.GetFrame(0);

            return;
        }

        _item = item;
        _icon.enabled = true;
        _icon.sprite = item.Icon;
        _name.text = item.ItemName;
        _frameImage.sprite = UIManager.Instance.GetFrame((int)_item.Rarity);
        _description.text = GenerateStatDescription(item);
    }

    private string GenerateStatDescription(ItemData item)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (item.Modifiers == null || item.Modifiers.Count == 0)
            return sb.ToString();

        foreach (var mod in item.Modifiers)
        {
            string statName = GetStatNameKorean(mod.StatType);
            string sign = mod.Value > 0 ? "+" : "";

            string colorHex = mod.Value > 0 ? "#55FF55" : "#FF5555";

            string statValue = mod.Value.ToString();
            if (mod.StatType == StatType.EXPGained || mod.StatType == StatType.CritChance || mod.StatType == StatType.CritDamageMultiplier)
                statValue = (mod.Value * 100).ToString("F0") + "%"; // 퍼센트로 표시

            sb.AppendLine($"<color={colorHex}>{statName} {sign}{statValue}</color>");
        }

        return sb.ToString().TrimEnd();
    }

    private string GetStatNameKorean(StatType type)
    {
        switch (type)
        {
            case StatType.AttackDamage: return "공격력";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.ProjectileSpeed: return "투사체 속도";
            case StatType.EXPGained: return "경험치 획득량";
            case StatType.CritChance: return "치명타 확률";
            case StatType.CritDamageMultiplier: return "치명타 데미지";
            default: return type.ToString();
        }
    }

    public void OnClick()
    {
        RewardManager.Instance.SelectItem(_item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item is null)
            return;

        UIManager.Instance.ShowItemInfo(_item, _pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideInfo();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_item is null)
            return;

        _curCoroutine = StartCoroutine(LongPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_curCoroutine != null) {
            StopCoroutine(_curCoroutine);
            UIManager.Instance.HideInfo();
            _curCoroutine = null;
        }
    }

    private IEnumerator LongPress()
    {
        yield return new WaitForSeconds(0.5f);

        if (_item != null)
            UIManager.Instance.ShowItemInfo(_item, _pos);
    }
}

