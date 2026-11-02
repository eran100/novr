using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOVR.VrUi.Native;

// Makes the bindings list scroll indicator thumb draggable. The VR cursor
// dispatches standard uGUI pointer events, so implementing the drag handlers
// keeps the mouse-only interaction pipeline intact. Dragging the thumb maps the
// pointer position along the track to the scroll content offset; the panel's
// existing ScrollRect.onValueChanged listener then keeps the thumb visuals in sync.
internal sealed class BindingScrollThumbDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect? _scrollRect;
    private RectTransform? _content;
    private RectTransform? _track;
    private float _maxScrollOffset;

    public void Setup(ScrollRect scrollRect, RectTransform content, RectTransform track, float contentHeight, float viewportHeight)
    {
        _scrollRect = scrollRect;
        _content = content;
        _track = track;
        _maxScrollOffset = Mathf.Max(0f, contentHeight - viewportHeight);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_scrollRect == null || _content == null || _track == null || _maxScrollOffset <= 0f) return;

        var camera = eventData.pressEventCamera ?? eventData.enterEventCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_track, eventData.position, camera, out var localPoint)) return;

        var trackHeight = _track.rect.height;
        if (trackHeight <= 0f) return;

        var scrollRatio = Mathf.Clamp01(0.5f - localPoint.y / trackHeight);
        _content.anchoredPosition = new Vector2(0f, scrollRatio * _maxScrollOffset);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
