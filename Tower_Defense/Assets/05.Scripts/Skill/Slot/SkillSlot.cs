using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private ActiveSkill _skill;
    [SerializeField] private RectTransform _pos;
    [SerializeField] private Image _image;

    Coroutine _curCoroutine;
    public void SetSkill(ActiveSkill skill)
    {
        _skill = skill;

        if (skill == null) {
            _image.sprite = null;
            gameObject.SetActive(false);
            return;
        }

        _image.sprite = _skill.data.icon;
        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_skill is null)
            return;

        UIManager.Instance.ShowSkillInfo(_skill, _pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideInfo();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_skill is null)
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

        if (_skill != null)
            UIManager.Instance.ShowSkillInfo(_skill, _pos);
    }
}
