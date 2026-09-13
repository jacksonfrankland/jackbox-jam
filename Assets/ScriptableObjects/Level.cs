using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
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
}
