using System;
using UnityEngine;
using PrimeTween;
using System.Collections.Generic;

public class Forklift : MonoBehaviour
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

    private void OnEnable()
    {
        Level.AddForklift(this);
    }

    private void OnDisable()
    {
        Level.RemoveForklift(this);
    }

    public void MoveForward(int value = 1)
    {
        var pushedSoFar = new List<Vector2Int>
        {
            Position
        };
        Level.PushForklift(Position + (Direction * value), Direction * value, pushedSoFar);
        Position += (Direction * value);
        var targetWorldPosition = TilesTransform.Value.position + new Vector3(Position.x, Position.y, 0);
        Tween.Position(transform, targetWorldPosition, .3f, Ease.InOutQuad);
    }


    public void RotateClockwise()
    {
        Direction = new Vector2Int(Direction.y, -Direction.x);
        Rotate();
    }

    public void RotateAnticlockwise()
    {
        Direction = new Vector2Int(-Direction.y, Direction.x);
        Rotate();
    }

    public void Rotate()
    {
        var angle = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0, 0, angle);
        Tween.Rotation(transform, targetRotation, .3f, Ease.InOutQuad);
    }
}
