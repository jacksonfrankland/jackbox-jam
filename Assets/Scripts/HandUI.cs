using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandUI : MonoBehaviour
{
    public Forklift TargetForklift;
    public CardUI[] CardSlots = new CardUI[Hand.HandSize];
    public Transform BoardRoot;
    public Collider2D DealButtonCollider;
    public Collider2D ConfirmMulliganButtonCollider;
    public float ShiftAnimationDuration = 0.15f;
    public float MulliganAnimationDuration = 0.3f;
    public float DragThreshold = 0.15f;
    public float GapBelowBoard = 1.5f;

    private readonly HashSet<int> _mulliganSelection = new();
    private readonly List<Vector3> _homeLocalPositions = new();
    private float _slotSpacing;
    private Camera _camera;
    private int _pressedCardIndex = -1;
    private int _draggedIndex = -1;
    private int _currentTargetIndex = -1;
    private bool _isDragging;
    private Vector3 _pressWorldPoint;

    private void Awake()
    {
        _camera = Camera.main;

        if (BoardRoot)
        {
            transform.SetParent(BoardRoot, worldPositionStays: false);
            PositionBelowBoard();
        }

        for (var i = 0; i < CardSlots.Length; i++)
        {
            CardSlots[i].SetIndex(i);
            _homeLocalPositions.Add(CardSlots[i].transform.localPosition);
        }
        _slotSpacing = CardSlots.Length > 1 ? _homeLocalPositions[1].x - _homeLocalPositions[0].x : 1f;

        RefreshDisplay();
    }

    private void Update()
    {
        if (!_camera || Mouse.current == null) return;

        var worldPoint = ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandlePress(worldPoint);
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            HandleHold(worldPoint);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleRelease();
        }
    }

    private void PositionBelowBoard()
    {
        var renderers = BoardRoot.GetComponentsInChildren<Renderer>();
        var hasBounds = false;
        var bounds = new Bounds();
        foreach (var candidate in renderers)
        {
            if (candidate.transform.IsChildOf(transform)) continue;
            if (!hasBounds)
            {
                bounds = candidate.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(candidate.bounds);
            }
        }
        if (!hasBounds) return;

        transform.localPosition = new Vector3(
            bounds.center.x - BoardRoot.position.x,
            bounds.min.y - BoardRoot.position.y - GapBelowBoard,
            0f);
    }

    private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
    {
        var distance = Mathf.Abs(_camera.transform.position.z - transform.position.z);
        return _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distance));
    }

    private void HandlePress(Vector3 worldPoint)
    {
        if (DealButtonCollider && DealButtonCollider.OverlapPoint(worldPoint))
        {
            DealHand();
            return;
        }
        if (ConfirmMulliganButtonCollider && ConfirmMulliganButtonCollider.OverlapPoint(worldPoint))
        {
            ConfirmMulligan();
            return;
        }

        for (var i = 0; i < CardSlots.Length; i++)
        {
            if (CardSlots[i].Collider && CardSlots[i].Collider.OverlapPoint(worldPoint))
            {
                _pressedCardIndex = i;
                _pressWorldPoint = worldPoint;
                _isDragging = false;
                return;
            }
        }
    }

    private void HandleHold(Vector3 worldPoint)
    {
        if (_pressedCardIndex < 0) return;

        if (!_isDragging)
        {
            if (Vector3.Distance(worldPoint, _pressWorldPoint) < DragThreshold) return;
            _isDragging = true;
            BeginDrag(_pressedCardIndex);
        }

        var localX = transform.InverseTransformPoint(worldPoint).x;
        var home = _homeLocalPositions[_pressedCardIndex];
        CardSlots[_pressedCardIndex].MoveTo(new Vector3(localX, home.y, home.z), 0f);
        UpdateDrag(_pressedCardIndex, localX);
    }

    private void HandleRelease()
    {
        if (_pressedCardIndex < 0) return;

        if (_isDragging)
        {
            EndDrag(_pressedCardIndex);
        }
        else
        {
            ToggleMulliganSelection(_pressedCardIndex);
        }

        _pressedCardIndex = -1;
        _isDragging = false;
    }

    public void DealHand()
    {
        if (!TargetForklift)
        {
            Debug.LogWarning($"{nameof(HandUI)} on {name} has no {nameof(TargetForklift)} assigned.", this);
            return;
        }
        TargetForklift.Hand.Deal();
        _mulliganSelection.Clear();
        RefreshDisplay();
    }

    public void ConfirmMulligan()
    {
        if (!TargetForklift)
        {
            Debug.LogWarning($"{nameof(HandUI)} on {name} has no {nameof(TargetForklift)} assigned.", this);
            return;
        }
        if (TargetForklift.Hand.HasMulliganed) return;
        var redrawnIndices = new List<int>(_mulliganSelection);
        TargetForklift.Hand.Mulligan(_mulliganSelection);
        _mulliganSelection.Clear();
        foreach (var i in redrawnIndices)
        {
            CardSlots[i].PlayMulliganFlip(TargetForklift.Hand.Cards[i], MulliganAnimationDuration);
        }
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

    private void BeginDrag(int index)
    {
        _draggedIndex = index;
        _currentTargetIndex = index;
        CardSlots[index].SetSortingOrder(20);
    }

    private void UpdateDrag(int index, float localX)
    {
        if (index != _draggedIndex) return;

        var targetIndex = ComputeTargetIndex(localX);
        if (targetIndex == _currentTargetIndex) return;
        _currentTargetIndex = targetIndex;

        for (var i = 0; i < CardSlots.Length; i++)
        {
            if (i == _draggedIndex) continue;
            var shift = 0;
            if (_draggedIndex < targetIndex && i > _draggedIndex && i <= targetIndex) shift = -1;
            else if (_draggedIndex > targetIndex && i >= targetIndex && i < _draggedIndex) shift = 1;
            CardSlots[i].MoveTo(_homeLocalPositions[i + shift], ShiftAnimationDuration);
        }
    }

    private void EndDrag(int index)
    {
        if (index != _draggedIndex) return;

        var draggedSlot = _draggedIndex;
        var targetSlot = _currentTargetIndex;
        _draggedIndex = -1;
        _currentTargetIndex = -1;
        CardSlots[draggedSlot].SetSortingOrder(10);

        if (targetSlot != draggedSlot && TargetForklift)
        {
            var cards = new List<CardType>(TargetForklift.Hand.Cards);
            var movedCard = cards[draggedSlot];
            cards.RemoveAt(draggedSlot);
            cards.Insert(targetSlot, movedCard);
            for (var i = 0; i < cards.Count; i++)
            {
                TargetForklift.Hand.Cards[i] = cards[i];
            }

            var slots = new List<CardUI>(CardSlots);
            var movedSlot = slots[draggedSlot];
            slots.RemoveAt(draggedSlot);
            slots.Insert(targetSlot, movedSlot);
            for (var i = 0; i < slots.Count; i++)
            {
                CardSlots[i] = slots[i];
                CardSlots[i].SetIndex(i);
            }
        }

        CardSlots[targetSlot].MoveTo(_homeLocalPositions[targetSlot], ShiftAnimationDuration);
    }

    private int ComputeTargetIndex(float localX)
    {
        for (var i = 0; i < _homeLocalPositions.Count; i++)
        {
            if (localX < _homeLocalPositions[i].x + _slotSpacing / 2f)
            {
                return i;
            }
        }
        return _homeLocalPositions.Count - 1;
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
