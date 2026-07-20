using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwapSlot : MonoBehaviour, IPointerClickHandler
{
    private int _slotIdx;
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private Image _select;
    [SerializeField] private ItemSwapUI _parentUI;

    public void SetSlot(int idx)
    {
        _slotIdx = idx;
        _select.gameObject.SetActive(false);

        if (idx < 0) {
            _image.sprite = null;
            _itemName.text = "";
            _image.rectTransform.localScale = Vector3.zero;
            return;
        }

        _image.sprite = InventoryManager.Instance.Items[_slotIdx].Icon;
        _itemName.text = InventoryManager.Instance.Items[_slotIdx].Name;
        _image.rectTransform.localScale = Vector3.one;
    }

    public void Select()
    {
        _select.gameObject.SetActive(true);
    }

    public void DeSelect()
    {
        _select.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _parentUI.SelectIndex(_slotIdx);
    }
}
