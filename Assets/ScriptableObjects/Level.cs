using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    private readonly List<Forklift> _forklifts = new();

    public Forklift GetForklift(Vector2Int position)
    {
        return GetForklift(position.x, position.y);
    }

    public Forklift GetForklift(int x, int y)
    {
        return _forklifts.Find(forklift => forklift.Position.x == x && forklift.Position.y == y);
    }

    public void AddForklift(Forklift forklift)
    {
        _forklifts.Add(forklift);
    }
}
