using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Item _item;
    [SerializeField] private RectTransform _pos;
    [SerializeField] private Image _image;

    Coroutine _curCoroutine;
    public void SetItem(Item item)
    {
        _item = item;

        if (item == null) {
            _image.sprite = null;
            return;
        }

        _image.sprite = _item.Data.Icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item is null) 
            return;

        UIManager.Instance.ShowItemInfo(_item.Data, _pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideItemInfo();
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
            UIManager.Instance.HideItemInfo();
            _curCoroutine = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item.Data.ItemType == ItemType.RandomBox)
            InventoryManager.Instance.Use(_item);
    }

    private IEnumerator LongPress()
    {
        yield return new WaitForSeconds(0.5f);

        if (_item != null)
            UIManager.Instance.ShowItemInfo(_item.Data, _pos);
    }
}
