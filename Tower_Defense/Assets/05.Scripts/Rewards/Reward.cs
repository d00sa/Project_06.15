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
    [SerializeField] private RectTransform _pos;
    [SerializeField] private ItemData _item;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Button _button;

    Coroutine _curCoroutine;
    private void Awake()
    {
        _pos = GetComponent<RectTransform>();
    }

    public void SetRewards(ItemData item)
    {
        if (item == null) {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _name.text = "";
            _button.interactable = false;
            return;
        }

        _item = item;
        _icon.enabled = true;
        _icon.sprite = item.Icon;
        _name.text = item.ItemName;
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

