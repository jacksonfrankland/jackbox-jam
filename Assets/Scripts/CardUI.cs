using PrimeTween;
using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    public SpriteRenderer Background;
    public TextMeshPro Label;
    public BoxCollider2D Collider;
    public Color NormalColor = Color.white;
    public Color SelectedColor = Color.yellow;

    private int _index;
    private Tween _moveTween;
    private Tween _flipTween;

    public int Index => _index;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public void SetCard(CardType card, bool selected)
    {
        Label.text = card.ToString();
        Background.color = selected ? SelectedColor : NormalColor;
    }

    public void SetSortingOrder(int order)
    {
        Background.sortingOrder = order;
        Label.sortingOrder = order + 1;
    }

    public void MoveTo(Vector3 localPosition, float duration)
    {
        if (_moveTween.isAlive) _moveTween.Stop();
        if (duration <= 0f)
        {
            transform.localPosition = localPosition;
            return;
        }
        var start = transform.localPosition;
        _moveTween = Tween.Custom(0f, 1f, duration, t =>
        {
            transform.localPosition = Vector3.Lerp(start, localPosition, t);
        }, Ease.InOutQuad);
    }

    public void PlayMulliganFlip(CardType newCard, float duration)
    {
        if (_flipTween.isAlive) _flipTween.Stop();
        var half = duration / 2f;
        _flipTween = Tween.Custom(1f, 0f, half, t =>
        {
            var scale = transform.localScale;
            scale.x = t;
            transform.localScale = scale;
        }, Ease.InQuad).OnComplete(() =>
        {
            SetCard(newCard, false);
            _flipTween = Tween.Custom(0f, 1f, half, t =>
            {
                var scale = transform.localScale;
                scale.x = t;
                transform.localScale = scale;
            }, Ease.OutQuad);
        });
    }
}
