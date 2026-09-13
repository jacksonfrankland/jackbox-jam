using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamaraFitToBounds : MonoBehaviour
{
    public TransformVariable Target;
    public float Padding = 0f;

    private Camera _camera;
    private Vector2Int _lastScreenDimensions;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        Fit();
    }

    private void Update()
    {
        Fit();
    }

    private void Fit()
    {
        if (Screen.width == _lastScreenDimensions.x && Screen.height == _lastScreenDimensions.y) return;
        if (Target == null) return;

        // calculate bounds
        var renderers = Target.Value.GetComponentsInChildren<Renderer>();
        var bounds = new Bounds(Target.Value.position, Vector3.zero);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i == 0) bounds = renderers[i].bounds;
            else bounds.Encapsulate(renderers[i].bounds);
        }

        // fit
        var dimensions = new Vector2(bounds.size.x + Padding * 2f, bounds.size.y + Padding * 2f);
        var contentAspect = dimensions.x / dimensions.y;
        var screenAspect = _camera.pixelWidth / (float)_camera.pixelHeight;
        _camera.orthographicSize = screenAspect >= contentAspect ? dimensions.y / 2f : (dimensions.x / screenAspect) / 2f;
        transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
        _lastScreenDimensions = new Vector2Int(Screen.width, Screen.height);
    }

}
