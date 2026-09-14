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
        Position += Direction * value;
        var targetWorldPosition = TilesTransform.Value.position + new Vector3(Position.x, Position.y, 0);
        Tween.Position(transform, targetWorldPosition, .3f, Ease.InOutQuad);
    }


    public void RotateClockwise()
    {
        Rotate(new Vector2Int(Direction.y, -Direction.x));
    }

    public void RotateAnticlockwise()
    {
        Rotate(new Vector2Int(-Direction.y, Direction.x));
    }

    public void Rotate(Vector2Int newDirection)
    {
        if (Level.RotationInProgress) return;
        Level.RotationInProgress = true;
        var rotatedSoFar = new List<Vector2Int>
        {
            Position
        };
        Level.RotateForklift(Position + Direction, newDirection, transform, rotatedSoFar);
        Level.GetForkliftsFacingPosition(Position).ForEach(forklift => Level.RotateForklift(forklift.Position, newDirection, transform, rotatedSoFar));

        Direction = newDirection;
        var angle = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0, 0, angle);
        Tween.Rotation(transform, targetRotation, .3f, Ease.InOutQuad).OnComplete(Level, level =>
        {
            level.ClearForkliftParents();
            level.SettleForklifts();
            Level.RotationInProgress = false;
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Level.RotationInProgress) return;

        if (transform.parent && !other.transform.parent)
        {
            other.transform.SetParent(transform.parent, worldPositionStays: true);
        }
    }
}
