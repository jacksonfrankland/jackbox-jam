using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    public Forklift TargetForklift;
    public CardUI[] CardSlots = new CardUI[Hand.HandSize];
    public float ShiftAnimationDuration = 0.15f;

    private readonly HashSet<int> _mulliganSelection = new();
    private readonly List<Vector2> _homePositions = new();
    private readonly List<float> _homeWorldX = new();
    private RectTransform _panelRect;
    private float _worldSlotSpacing;
    private int _draggedIndex = -1;
    private int _currentTargetIndex = -1;

    private void Start()
    {
        _panelRect = (RectTransform)CardSlots[0].transform.parent;

        var layoutGroup = _panelRect.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panelRect);
            layoutGroup.enabled = false;
        }

        for (var i = 0; i < CardSlots.Length; i++)
        {
            CardSlots[i].Init(this, i);
            _homePositions.Add(((RectTransform)CardSlots[i].transform).anchoredPosition);
            _homeWorldX.Add(((RectTransform)CardSlots[i].transform).position.x);
        }
        _worldSlotSpacing = CardSlots.Length > 1 ? _homeWorldX[1] - _homeWorldX[0] : 1f;

        RefreshDisplay();
    }

    public void DealHand()
    {
        if (!TargetForklift) return;
        TargetForklift.Hand.Deal();
        _mulliganSelection.Clear();
        RefreshDisplay();
    }

    public void ConfirmMulligan()
    {
        if (!TargetForklift) return;
        TargetForklift.Hand.Mulligan(_mulliganSelection);
        _mulliganSelection.Clear();
        RefreshDisplay();
    }

    public void ToggleMulliganSelection(int index)
    {
        if (!TargetForklift || TargetForklift.Hand.HasMulliganed) return;
        if (!_mulliganSelection.Remove(index))
        {
            _mulliganSelection.Add(index);
        }
        RefreshDisplay();
    }

    public void BeginDrag(int index)
    {
        _draggedIndex = index;
        _currentTargetIndex = index;
    }

    public void UpdateDrag(int index, Vector3 pointerWorldPosition)
    {
        if (index != _draggedIndex) return;

        var targetIndex = ComputeTargetIndex(pointerWorldPosition.x);
        if (targetIndex == _currentTargetIndex) return;
        _currentTargetIndex = targetIndex;

        for (var i = 0; i < CardSlots.Length; i++)
        {
            if (i == _draggedIndex) continue;
            var shift = 0;
            if (_draggedIndex < targetIndex && i > _draggedIndex && i <= targetIndex) shift = -1;
            else if (_draggedIndex > targetIndex && i >= targetIndex && i < _draggedIndex) shift = 1;
            CardSlots[i].MoveTo(_homePositions[i + shift], ShiftAnimationDuration);
        }
    }

    public void EndDrag(int index)
    {
        if (index != _draggedIndex) return;

        if (_currentTargetIndex != _draggedIndex && TargetForklift)
        {
            var cards = new List<CardType>(TargetForklift.Hand.Cards);
            var moved = cards[_draggedIndex];
            cards.RemoveAt(_draggedIndex);
            cards.Insert(_currentTargetIndex, moved);
            for (var i = 0; i < cards.Count; i++)
            {
                TargetForklift.Hand.Cards[i] = cards[i];
            }
        }

        _draggedIndex = -1;
        _currentTargetIndex = -1;
        RefreshDisplay();
        for (var i = 0; i < CardSlots.Length; i++)
        {
            CardSlots[i].MoveTo(_homePositions[i], ShiftAnimationDuration);
        }
    }

    private int ComputeTargetIndex(float worldX)
    {
        for (var i = 0; i < _homeWorldX.Count; i++)
        {
            if (worldX < _homeWorldX[i] + _worldSlotSpacing / 2f)
            {
                return i;
            }
        }
        return _homeWorldX.Count - 1;
    }

    private void RefreshDisplay()
    {
        if (!TargetForklift)
        {
            Debug.LogWarning($"{nameof(HandUI)} on {name} has no {nameof(TargetForklift)} assigned.", this);
            return;
        }

        for (var i = 0; i < CardSlots.Length; i++)
        {
            CardSlots[i].SetCard(TargetForklift.Hand.Cards[i], _mulliganSelection.Contains(i));
        }
    }
}
