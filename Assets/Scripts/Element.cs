using System;
using UnityEngine;

public class Element : MonoBehaviour
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public TransformVariable TilesTransform;
    public Level Level;
    public virtual bool CanBePushed => true;

    private void Awake()
    {
        Position.x = (int)Math.Round(transform.position.x);
        Position.y = (int)Math.Round(transform.position.y);
        transform.position = TilesTransform.Value.position + new Vector3(Position.x, Position.y, 0);
        Direction = Vector2Int.RoundToInt(transform.up) * -1;
    }

    private void OnEnable()
    {
        Level.AddElement(this);
    }

    private void OnDisable()
    {
        Level.RemoveElement(this);
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
