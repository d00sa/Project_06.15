using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    private Item _item;
    [SerializeField] private Image _image;

    public void SetItem(Item item)
    {
        _item = item;

        if (item == null)
            return;

        _image.sprite = _item.Data.Icon;
    }
}
