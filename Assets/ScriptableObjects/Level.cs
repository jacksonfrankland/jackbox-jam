using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    public TransformVariable TilesTransform;

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
        _elements.RemoveAll(element => !element);
        foreach (var element in _elements)
        {
            if (element is T typed && typed.Position.x == x && typed.Position.y == y)
            {
                return typed;
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

    public void PushForklift(Vector2Int targetPosition, Vector2Int pushDirection, List<Vector2Int> pushedSoFar)
    {
        if (pushedSoFar.Contains(targetPosition)) return;
        var otherForklift = GetElement<Forklift>(targetPosition);
        if (!otherForklift) return;

        pushedSoFar.Add(targetPosition);
        PushForklift(targetPosition + pushDirection, pushDirection, pushedSoFar);
        if ((otherForklift.Direction.x == pushDirection.y && otherForklift.Direction.y == -pushDirection.x)
            || (otherForklift.Direction.x == -pushDirection.y && otherForklift.Direction.y == pushDirection.x))
        {
            PushForklift(targetPosition + otherForklift.Direction, pushDirection, pushedSoFar);
        }

        otherForklift.Position += pushDirection;
        var targetWorldPosition = TilesTransform.Value.position + new Vector3(otherForklift.Position.x, otherForklift.Position.y, 0);
        Tween.Position(otherForklift.GetComponent<Transform>(), targetWorldPosition, .3f, Ease.InOutQuad);
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

    public void ClearForkliftParents()
    {
        _elements.ForEach(forklift => forklift.transform.SetParent(null, true));
    }

    public void SettleForklifts()
    {
        _elements.ForEach(forklift =>
        {
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
    }

    public void RotateForklift(Vector2Int targetPosition, Vector2Int rotationDirection, Transform pivot, List<Vector2Int> rotatedSoFar)
    {
        if (rotatedSoFar.Contains(targetPosition)) return;
        rotatedSoFar.Add(targetPosition);
        var forklift = GetElement<Forklift>(targetPosition);
        if (!forklift) return;

        forklift.transform.SetParent(pivot, worldPositionStays: true);

        if (forklift.Direction != (rotationDirection * -1))
        {
            RotateForklift(forklift.Position + forklift.Direction, rotationDirection, pivot, rotatedSoFar);
        }

        GetElementsFacingPosition<Forklift>(forklift.Position)
        .FindAll(f => f.Direction != rotationDirection)
        .ForEach(f => RotateForklift(f.Position, rotationDirection, pivot, rotatedSoFar));
    }
}
