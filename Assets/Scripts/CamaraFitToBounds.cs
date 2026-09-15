using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamaraFitToBounds : MonoBehaviour
{
    public TransformVariable Target;
    public float Padding = 0f;
    public float BottomMarginPixels = 0f;

    private Camera _camera;
    private Vector2Int _lastScreenDimensions;
    private float _lastBottomMarginPixels;

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
        if (Screen.width == _lastScreenDimensions.x && Screen.height == _lastScreenDimensions.y && BottomMarginPixels == _lastBottomMarginPixels) return;
        if (Target == null) return;

        var marginFraction = Mathf.Clamp01(BottomMarginPixels / Screen.height);
        _camera.rect = new Rect(0f, marginFraction, 1f, 1f - marginFraction);

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

        float cameraY;
        if (screenAspect >= contentAspect)
        {
            _camera.orthographicSize = dimensions.y / 2f;
            cameraY = bounds.center.y;
        }
        else
        {
            _camera.orthographicSize = (dimensions.x / screenAspect) / 2f;
            // any leftover vertical space collects at the bottom instead of being split above and below
            cameraY = (bounds.max.y + Padding) - _camera.orthographicSize;
        }

        transform.position = new Vector3(bounds.center.x, cameraY, -10f);
        _lastScreenDimensions = new Vector2Int(Screen.width, Screen.height);
        _lastBottomMarginPixels = BottomMarginPixels;
    }

}
