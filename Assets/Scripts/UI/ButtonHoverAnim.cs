using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _rectTransform;
    private Tween _hoverTween;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverTween?.Kill();
        _hoverTween = _rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverTween?.Kill();
        _hoverTween = _rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }
}
