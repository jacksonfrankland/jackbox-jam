using UnityEngine;

public class TransformToVariable : MonoBehaviour
{
    public TransformVariable Variable;

    private void Awake()
    {
        Variable.Value = transform;
    }
}
