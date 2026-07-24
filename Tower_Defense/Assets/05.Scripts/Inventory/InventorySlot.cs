using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Item _item;
    [Header("[슬롯 설정]")]
    [SerializeField] private RectTransform _pos;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Image _image;
    [SerializeField] private Image _backgroundImage;

    Coroutine _curCoroutine;

    public void SetItem(Item item)
    {
        _item = item;

        if (item == null) {
            _image.sprite = null;
            _name.text = "";
            _description.text = "";
            _image.rectTransform.localScale = Vector3.zero;
            _backgroundImage.sprite = UIManager.Instance.GetFrame(0);

            return;
        }

        _image.sprite = _item.Icon;
        _name.text = _item.Name;
        _description.text = GenerateStatDescription(_item.Data);
        _image.rectTransform.localScale = Vector3.one;
        _backgroundImage.sprite = UIManager.Instance.GetFrame((int)_item.Rarity);
    }

    private string GenerateStatDescription(ItemData item)
    {
        bool isLongText = item.ItemType == ItemType.Consumable ||
                                  (item.ItemType == ItemType.Equipment && item.Modifiers != null && item.Modifiers.Count >= 3);

        string sizeOpen = isLongText ? "<size=80%>" : "";
        string sizeClose = isLongText ? "</size>" : "";

        if (item.ItemType == ItemType.Consumable || item.ItemType == ItemType.RandomBox)
        {
            return $"{sizeOpen}{item.Description}{sizeClose}";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (item.Modifiers == null || item.Modifiers.Count == 0)
            return sb.ToString();

        for (int i = 0; i < item.Modifiers.Count; i++)
        {
            var mod = item.Modifiers[i];
            string statName = GetStatNameKorean(mod.StatType);
            string sign = mod.Value > 0 ? "+" : "";

            string colorHex = mod.Value > 0 ? "#55FF55" : "#FF5555";

            string statValue = mod.Value.ToString();

            // 퍼센트로 표시할 스탯들 처리
            if (mod.StatType == StatType.EXPGained ||
                mod.StatType == StatType.CritChance ||
                mod.StatType == StatType.CritDamageMultiplier ||
                mod.StatType == StatType.AttackSpeed ||
                mod.StatType == StatType.Luck)
            {
                statValue = (mod.Value * 100).ToString("F0") + "%";
            }

            sb.Append($"<color={colorHex}>{statName} {sign}{statValue}</color>");

            if (i < item.Modifiers.Count - 1)
            {
                sb.Append(", ");
            }
        }

        return $"{sizeOpen}{sb.ToString().TrimEnd()}{sizeClose}";
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
            case StatType.Luck: return "행운";
            default: return type.ToString();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item is null) 
            return;

        UIManager.Instance.ShowItemInfo(_item.Data, _pos);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item.Type == ItemType.RandomBox || _item.Type == ItemType.Consumable)
            InventoryManager.Instance.Use(_item);
    }

    private IEnumerator LongPress()
    {
        yield return new WaitForSeconds(0.5f);

        if (_item != null)
            UIManager.Instance.ShowItemInfo(_item.Data, _pos);
    }
}
