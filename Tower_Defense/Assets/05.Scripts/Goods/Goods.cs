using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Goods : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ItemData _item;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _price;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Button _button;

    public void SetGoods(ItemData item)
    {
        if (item == null) {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _price.text = "";
            _name.text = "";
            _button.interactable = false;
            return;
        }

        _item = item;
        _icon.enabled = true;
        _icon.sprite = item.Icon;
        _name.text = item.ItemName;
        _price.text = $"{item.Price:N0}$";

        Refresh();
    }

    public void OnClick()
    {
        StoreManager.Instance.TryBuy(_item);
    }

    public void Refresh()
    {
        if (_item == null)
            return;

        bool canBuy = StoreManager.Instance.CanBuy(_item);

        _button.interactable = canBuy;
        _icon.color = canBuy ? Color.white : new Color32(150, 150, 150, 255);
        _price.color = canBuy ? Color.white : Color.red;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}

