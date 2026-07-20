using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ItemSwapUI : MonoBehaviour
{
    [SerializeField] private Button _swapButton;      // 교체 버튼
    [SerializeField] private Button _discardButton;   // 새 아이템 버리기 버튼
    [SerializeField] List<SwapSlot> _slots; //교체 슬롯들
    private RectTransform _rect;

    private int _selectedIndex = -1; // 현재 유저가 선택한 슬롯 인덱스 (-1은 미선택)
    private Action<bool, int> onResultCallback;

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _swapButton.onClick.AddListener(() => OnClickSwap(_selectedIndex));
        _discardButton.onClick.AddListener(() => OnClickCancel());
    }
    public void OpenWindow(ItemData newItem, Action<bool, int> callback)
    {
        this.onResultCallback = callback;
        Time.timeScale = 0f;
        
        //슬롯 설정
        for (int i = 0; i < InventoryManager.Instance.Count; i++)
            _slots[i].SetSlot(i);

        _rect.localScale = Vector3.one;
    }
    /// <summary>유저가 교체 버튼을 클릭 했을 떄.</summary>
    public void OnClickSwap(int selectedIndex)
    {
        if (selectedIndex < 0) 
            return;

        onResultCallback?.Invoke(true, selectedIndex); // 교체 + 인덱스 반환
        Close();
    }
    /// <summary> 유저가 '포기/버리기' 버튼을 눌렀을 때 </summary>
    public void OnClickCancel()
    {
        onResultCallback?.Invoke(false, -1); // 교체 취소
        Close();
    }
    public void SelectIndex(int idx)
    {
        if (idx < 0) 
            return;

        //자기 자신을 선택했다면
        if (_selectedIndex == idx) {
            _slots[idx].DeSelect();
            _selectedIndex = -1;
            return;
        }
        //다른 걸 선택했고 이전에 선택된 게 있을 때.
        else if (_selectedIndex >= 0)
            _slots[_selectedIndex].DeSelect();
        
        _selectedIndex = idx;
        _slots[idx].Select();
    }
    private void Close()
    {
        _rect.localScale = Vector3.zero;
        Time.timeScale = GameManager.Instance.CurSpeed;
    }
} 
