using System.Collections.Generic;
using UnityEngine;

namespace NOVR.VrUi.SpecialBehavior;

public class NOVRMainMenuBehavior : UIRenderedCanvasBehavior
{
    private const string OverlayMenuLayerName = "OverlayMenuLayer";
    private Transform? _overlayMenuLayer;
    private readonly List<(GameObject Target, bool WasActive)> _hiddenForOverlay = new();
    private bool _hidingForOverlay;
    private int _lastOverlayChildCount = -1;

    private void Start()
    {
        transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
        transform.localPosition = new Vector3(0f, 0f, 3f);
        CacheOverlayMenuLayer();
    }

    private void LateUpdate()
    {
        if (_overlayMenuLayer == null)
        {
            CacheOverlayMenuLayer();
            if (_overlayMenuLayer == null) return;
        }

        var overlayChildCount = _overlayMenuLayer.childCount;
        var shouldHideMainChrome = overlayChildCount > 0;
        if (shouldHideMainChrome == _hidingForOverlay && overlayChildCount == _lastOverlayChildCount)
        {
            return;
        }

        _lastOverlayChildCount = overlayChildCount;

        if (shouldHideMainChrome)
        {
            HideMainChromeForOverlay();
        }
        else
        {
            RestoreMainChromeAfterOverlay();
        }
    }

    private void OnDisable()
    {
        RestoreMainChromeAfterOverlay();
    }

    private void CacheOverlayMenuLayer()
    {
        _overlayMenuLayer = FindChildByName(transform, OverlayMenuLayerName);
    }

    private void HideMainChromeForOverlay()
    {
        if (!_hidingForOverlay)
        {
            _hiddenForOverlay.Clear();
        }

        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child == null || child == _overlayMenuLayer) continue;
            if (!child.gameObject.activeSelf) continue;
            if (_hidingForOverlay && IsTracked(child.gameObject)) continue;

            _hiddenForOverlay.Add((child.gameObject, true));
            child.gameObject.SetActive(false);
        }

        _hidingForOverlay = true;
    }

    private void RestoreMainChromeAfterOverlay()
    {
        if (!_hidingForOverlay) return;

        for (var i = 0; i < _hiddenForOverlay.Count; i++)
        {
            var entry = _hiddenForOverlay[i];
            if (entry.Target != null && entry.WasActive)
            {
                entry.Target.SetActive(true);
            }
        }

        _hiddenForOverlay.Clear();
        _hidingForOverlay = false;
    }

    private bool IsTracked(GameObject gameObject)
    {
        for (var i = 0; i < _hiddenForOverlay.Count; i++)
        {
            if (_hiddenForOverlay[i].Target == gameObject) return true;
        }

        return false;
    }

    private static Transform? FindChildByName(Transform root, string childName)
    {
        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.name == childName) return child;

            var nested = FindChildByName(child, childName);
            if (nested != null) return nested;
        }

        return null;
    }
}
