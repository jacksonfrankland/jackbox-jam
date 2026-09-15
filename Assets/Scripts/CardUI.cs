using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI Label;
    public Image Background;
    public Color NormalColor = Color.white;
    public Color SelectedColor = Color.yellow;

    private HandUI _handUI;
    private int _index;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private Transform _dragStartParent;
    private Vector3 _dragOffset;
    private Tween _moveTween;

    public void Init(HandUI handUI, int index)
    {
        _handUI = handUI;
        _index = index;
        _rectTransform = (RectTransform)transform;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (!_canvasGroup)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = (RectTransform)_canvas.transform;
    }

    public void SetCard(CardType card, bool selected)
    {
        Label.text = card.ToString();
        Background.color = selected ? SelectedColor : NormalColor;
    }

    public void MoveTo(Vector2 anchoredPosition, float duration)
    {
        if (_moveTween.isAlive) _moveTween.Stop();
        if (duration <= 0f)
        {
            _rectTransform.anchoredPosition = anchoredPosition;
            return;
        }
        var start = _rectTransform.anchoredPosition;
        _moveTween = Tween.Custom(0f, 1f, duration, t =>
        {
            _rectTransform.anchoredPosition = Vector2.Lerp(start, anchoredPosition, t);
        }, Ease.InOutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _handUI.ToggleMulliganSelection(_index);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartParent = transform.parent;
        _canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root, worldPositionStays: true);
        transform.SetAsLastSibling();

        var cameraForConversion = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, cameraForConversion, out var worldPoint);
        _dragOffset = _rectTransform.position - worldPoint;

        _handUI.BeginDrag(_index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var cameraForConversion = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, cameraForConversion, out var worldPoint);
        _rectTransform.position = worldPoint + _dragOffset;
        _handUI.UpdateDrag(_index, worldPoint);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        transform.SetParent(_dragStartParent, worldPositionStays: false);
        _handUI.EndDrag(_index);
    }
}
