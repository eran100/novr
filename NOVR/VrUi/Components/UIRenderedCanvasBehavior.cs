using UnityEngine;

namespace NOVR.VrUi.SpecialBehavior;

public class UIRenderedCanvasBehavior : MonoBehaviour
{
    private const float DynamicLayerSweepInterval = 0.25f;

    private bool _initialized;
    private float _nextLayerSweepTime;
    
    protected virtual bool ShouldInitializeCanvas => true;

    public virtual void Awake() => Initialize();

    public virtual void OnEnable()
    {
        Initialize();
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
            VrCanvasHitTester.Register(canvas);
    }

    public virtual void OnDisable()
    {
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
            VrCanvasHitTester.Unregister(canvas);
    }

    // Dynamic children (mission list rows, Rewired ControlMapper rows, dropdowns) are
    // instantiated long after Initialize() and keep their prefab layer (usually UI),
    // which the VR UI camera's culling mask does not render. The initial recursive
    // layer pass in Initialize() cannot see them, so re-apply the VrUi layer on a
    // throttled cadence instead of scanning every frame.
    private void LateUpdate()
    {
        if (!ShouldInitializeCanvas) return;
        if (Time.unscaledTime < _nextLayerSweepTime) return;
        _nextLayerSweepTime = Time.unscaledTime + DynamicLayerSweepInterval;
        ApplyVrUiLayerRecursive(transform);
    }

    private void Initialize()
    {
        if (!ShouldInitializeCanvas) return;
        if (_initialized) return;
        _initialized = true;
        
        
        ApplyVrUiLayerRecursive(transform);
        transform.localPosition = transform.localPosition with { z = 0 };
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) return;
        
        
        canvas.renderMode = RenderMode.WorldSpace;
        Debug.Log($"{GetType().Name}: Set canvas render mode of {canvas.gameObject.name}. Is currently:  {canvas.renderMode}");
        canvas.worldCamera = APIBus.CockpitHudCamera;
        Debug.Log($"{GetType().Name}: Set canvas world camera of {canvas.gameObject.name}. Is currently:  {canvas.worldCamera}");

        VrCanvasHitTester.Register(canvas);
    }
    
    
    private static void ApplyVrUiLayerRecursive(Transform root)
    {
        LayerHelper.SetLayerRecursive(root, LayerHelper.GetVrUiLayer());
    }


    protected static Transform FindChildStartingWith(Transform parent, string childNamePrefix)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name.StartsWith(childNamePrefix))
            {
                return child;
            }

            var nestedChild = FindChildStartingWith(child, childNamePrefix);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}
