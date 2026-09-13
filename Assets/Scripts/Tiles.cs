using UnityEngine;

public class Tiles : MonoBehaviour
{
    public TransformVariable TransformVariable;

    private void Awake()
    {
        TransformVariable.Value = transform;
    }
}
