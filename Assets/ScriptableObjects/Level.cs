using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    public TransformVariable TilesTransform;

    private readonly List<Forklift> _forklifts = new();

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
        _forklifts.Clear();
    }

    public Forklift GetForklift(Vector2Int position)
    {
        return GetForklift(position.x, position.y);
    }

    public Forklift GetForklift(int x, int y)
    {
        _forklifts.RemoveAll(forklift => !forklift);
        return _forklifts.Find(forklift => forklift.Position.x == x && forklift.Position.y == y);
    }

    public void AddForklift(Forklift forklift)
    {
        _forklifts.RemoveAll(f => !f);
        if (!_forklifts.Contains(forklift))
        {
            _forklifts.Add(forklift);
        }
    }

    public void RemoveForklift(Forklift forklift)
    {
        _forklifts.Remove(forklift);
    }

    public void LogForkliftDetails(string context)
    {
        Debug.Log($"[Level] {context}");
        foreach (var forklift in _forklifts)
        {
            Debug.Log($"  {forklift.name}: Position={forklift.Position}, Direction={forklift.Direction}");
        }
    }

    public void PushForklift(Vector2Int targetPosition, Vector2Int pushDirection, List<Vector2Int> pushedSoFar)
    {
        if (pushedSoFar.Contains(targetPosition)) return;
        var otherForklift = GetForklift(targetPosition);
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

}
