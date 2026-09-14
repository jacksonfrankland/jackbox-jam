using System;
using UnityEngine;

public class Element : MonoBehaviour
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public TransformVariable TilesTransform;
    public Level Level;

    private void Awake()
    {
        Position.x = (int)Math.Round(transform.position.x);
        Position.y = (int)Math.Round(transform.position.y);
        transform.position = TilesTransform.Value.position + new Vector3(Position.x, Position.y, 0);
        Direction = Vector2Int.RoundToInt(transform.up) * -1;
    }

}
