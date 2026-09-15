using System;
using UnityEngine;
using PrimeTween;
using System.Collections.Generic;

public class Forklift : Element
{
    public Hand Hand = new();

    public void MoveForward(int value = 1)
    {
        var pushedSoFar = new List<Vector2Int>
        {
            Position
        };
        Level.PushElement(Position + (Direction * value), Direction * value, pushedSoFar);
        Position += Direction * value;
        var targetWorldPosition = TilesTransform.Value.position + new Vector3(Position.x, Position.y, 0);
        Tween.Position(transform, targetWorldPosition, Level.AnimationSpeed, Ease.InOutQuad);
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
        Level.GetElementsFacingPosition<Forklift>(Position).ForEach(forklift => Level.RotateForklift(forklift.Position, newDirection, transform, rotatedSoFar));

        Direction = newDirection;
        var angle = Mathf.Atan2(Direction.x, -Direction.y) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0, 0, angle);
        Tween.Rotation(transform, targetRotation, Level.AnimationSpeed, Ease.InOutQuad).OnComplete(Level, level =>
        {
            var affected = level.GetElementsParentedTo(transform);
            level.ClearForkliftParents(affected);
            level.SettleForklifts(affected);
            Level.RotationInProgress = false;
        });
    }

}
