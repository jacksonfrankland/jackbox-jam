using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    public TransformVariable TilesTransform;
    public float AnimationSpeed = .3f;

    private readonly List<Element> _elements = new();
    public bool RotationInProgress = false;

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        _elements.Clear();
    }

    public T GetElement<T>(Vector2Int position) where T : Element
    {
        return GetElement<T>(position.x, position.y);
    }

    public T GetElement<T>(int x, int y) where T : Element
    {
        foreach (var element in _elements)
        {
            if (element && element is T typed && typed.Position.x == x && typed.Position.y == y)
            {
                return typed;
            }
        }
        return null;
    }

    public Element GetPushableElement(Vector2Int position)
    {
        foreach (var element in _elements)
        {
            if (element && element.CanBePushed && element.Position == position)
            {
                return element;
            }
        }
        return null;
    }

    public void AddElement(Element element)
    {
        _elements.RemoveAll(f => !f);
        if (!_elements.Contains(element))
        {
            _elements.Add(element);
        }
    }

    public void RemoveElement(Element element)
    {
        _elements.Remove(element);
    }

    public void LogForkliftDetails(string context)
    {
        Debug.Log($"[Level] {context}");
        foreach (var forklift in _elements)
        {
            Debug.Log($"  {forklift.name}: Position={forklift.Position}, Direction={forklift.Direction}");
        }
    }

    public void PushElement(Vector2Int targetPosition, Vector2Int pushDirection, List<Vector2Int> pushedSoFar)
    {
        if (pushedSoFar.Contains(targetPosition)) return;
        var otherElement = GetPushableElement(targetPosition);
        if (!otherElement) return;

        pushedSoFar.Add(targetPosition);
        PushElement(targetPosition + pushDirection, pushDirection, pushedSoFar);
        if (otherElement is Forklift && ((otherElement.Direction.x == pushDirection.y && otherElement.Direction.y == -pushDirection.x)
            || (otherElement.Direction.x == -pushDirection.y && otherElement.Direction.y == pushDirection.x)))
        {
            PushElement(targetPosition + otherElement.Direction, pushDirection, pushedSoFar);
        }

        otherElement.Position += pushDirection;
        var targetWorldPosition = TilesTransform.Value.position + new Vector3(otherElement.Position.x, otherElement.Position.y, 0);
        Tween.Position(otherElement.GetComponent<Transform>(), targetWorldPosition, AnimationSpeed, Ease.InOutQuad);
    }

    public List<T> GetElementsFacingPosition<T>(Vector2Int targetPosition) where T : Element
    {
        var result = new List<T>();
        foreach (var element in _elements)
        {
            if (element is T typed && targetPosition == typed.Position + typed.Direction)
            {
                result.Add(typed);
            }
        }
        return result;
    }

    public List<Element> GetElementsParentedTo(Transform pivot)
    {
        var result = new List<Element>();
        foreach (var element in _elements)
        {
            if (element && element.transform.parent == pivot)
            {
                result.Add(element);
            }
        }
        return result;
    }

    public void ClearForkliftParents(List<Element> affected)
    {
        affected.ForEach(element => element.transform.SetParent(null, true));
    }

    public void SettleForklifts(List<Element> affected)
    {
        var rawPositions = new Dictionary<Element, Vector3>();
        affected.ForEach(forklift =>
        {
            rawPositions[forklift] = forklift.transform.position;
            forklift.Position.x = (int)Math.Round(forklift.transform.position.x);
            forklift.Position.y = (int)Math.Round(forklift.transform.position.y);
            forklift.transform.position = TilesTransform.Value.position + new Vector3(forklift.Position.x, forklift.Position.y, 0);
            var rawUp = forklift.transform.up;
            var rawAngle = Mathf.Atan2(-rawUp.x, rawUp.y) * Mathf.Rad2Deg;
            var snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
            var rad = snappedAngle * Mathf.Deg2Rad;
            forklift.Direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sin(rad)), Mathf.RoundToInt(-Mathf.Cos(rad)));
            forklift.transform.rotation = Quaternion.Euler(0, 0, snappedAngle);
        });

        ResolveOverlaps(affected, rawPositions);
    }

    private void ResolveOverlaps(List<Element> affected, Dictionary<Element, Vector3> rawPositions)
    {
        var affectedSet = new HashSet<Element>(affected);

        var groups = new Dictionary<Vector2Int, List<Element>>();
        foreach (var element in _elements)
        {
            if (!element.CanBePushed) continue;
            if (!groups.TryGetValue(element.Position, out var group))
            {
                group = new List<Element>();
                groups[element.Position] = group;
            }
            group.Add(element);
        }

        var occupied = new HashSet<Vector2Int>(groups.Keys);

        foreach (var (position, elements) in groups)
        {
            if (elements.Count <= 1) continue;
            // a conflict that doesn't involve anything from this rotation was already
            // resolved by a previous settle, no need to redo the work for it
            if (!elements.Exists(affectedSet.Contains)) continue;

            // whichever element's raw (pre-round) position was actually closest to this
            // tile keeps it, the rest get displaced to the nearest free tile instead
            var targetWorld = TilesTransform.Value.position + new Vector3(position.x, position.y, 0);
            Vector3 RawOf(Element e) => rawPositions.TryGetValue(e, out var raw) ? raw : e.transform.position;
            elements.Sort((a, b) => Vector3.Distance(RawOf(a), targetWorld)
                .CompareTo(Vector3.Distance(RawOf(b), targetWorld)));

            for (var i = 1; i < elements.Count; i++)
            {
                var element = elements[i];
                var travelDirection = GetTravelDirection(RawOf(element), targetWorld);
                var freePosition = FindFreePositionInDirection(position, travelDirection, occupied);
                element.Position = freePosition;
                element.transform.position = TilesTransform.Value.position + new Vector3(freePosition.x, freePosition.y, 0);
                occupied.Add(freePosition);
            }
        }
    }

    private static Vector2Int GetTravelDirection(Vector3 fromWorld, Vector3 toWorld)
    {
        var delta = toWorld - fromWorld;
        return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
            ? new Vector2Int(delta.x >= 0 ? 1 : -1, 0)
            : new Vector2Int(0, delta.y >= 0 ? 1 : -1);
    }

    private static Vector2Int FindFreePositionInDirection(Vector2Int origin, Vector2Int direction, HashSet<Vector2Int> occupied)
    {
        var candidate = origin;
        for (var step = 0; step < 50; step++)
        {
            candidate += direction;
            if (!occupied.Contains(candidate))
            {
                return candidate;
            }
        }
        return origin;
    }

    public void RotateForklift(Vector2Int targetPosition, Vector2Int rotationDirection, Transform pivot, List<Vector2Int> rotatedSoFar)
    {
        if (rotatedSoFar.Contains(targetPosition)) return;
        rotatedSoFar.Add(targetPosition);
        var element = GetPushableElement(targetPosition);
        if (!element) return;

        element.transform.SetParent(pivot, worldPositionStays: true);

        if (element is Forklift && element.Direction != (rotationDirection * -1))
        {
            RotateForklift(element.Position + element.Direction, rotationDirection, pivot, rotatedSoFar);
        }

        GetElementsFacingPosition<Forklift>(element.Position)
        .FindAll(f => f.Direction != rotationDirection)
        .ForEach(f => RotateForklift(f.Position, rotationDirection, pivot, rotatedSoFar));
    }

    public void RunConveyerBelts()
    {
        List<Vector2Int> previousPositions = new();
        foreach (var element in _elements)
        {
            if (!element.CanBePushed) continue;
            if (previousPositions.Contains(element.Position)) continue;
            var conveyerBelt = GetElement<ConveyerBelt>(element.Position);
            if (!conveyerBelt) continue;

            previousPositions.Add(element.Position);
            PushElement(element.Position, conveyerBelt.Direction, new List<Vector2Int> { });
        }
    }
}
