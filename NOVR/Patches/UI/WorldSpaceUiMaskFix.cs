using UnityEngine;
using UnityEngine.UI;

namespace NOVR.Patches.UI;

/// <summary>
/// World-space canvases break Unity stencil <see cref="Mask"/> for TMP.
/// Replace enabled Masks with <see cref="RectMask2D"/> so text stays visible and clipping still works.
/// </summary>
internal static class WorldSpaceUiMaskFix
{
    public static int ReplaceMasksWithRectMask2D(Transform root)
    {
        if (root == null) return 0;

        var masks = root.GetComponentsInChildren<Mask>(true);
        var replaced = 0;
        for (var i = 0; i < masks.Length; i++)
        {
            var mask = masks[i];
            if (mask == null || !mask.enabled) continue;
            if (ReplaceMask(mask))
                replaced++;
        }

        return replaced;
    }

    public static bool EnsureRectMaskClipping(Transform from)
    {
        if (from == null) return false;

        var scrollRect = from.GetComponentInParent<ScrollRect>(true);
        var mask = from.GetComponentInParent<Mask>(true);
        var viewport = mask != null
            ? mask.gameObject
            : scrollRect != null && scrollRect.viewport != null
                ? scrollRect.viewport.gameObject
                : null;
        if (viewport == null) return false;

        if (mask != null)
            ReplaceMask(mask);
        else
            EnsureRectMask(viewport);

        return true;
    }

    private static bool ReplaceMask(Mask mask)
    {
        var viewport = mask.gameObject;
        // Mask with showMaskGraphic=false hides the Image used for stencil.
        // Removing Mask makes that Image visible again (e.g. white slab over keybind Actions).
        var hideMaskGraphic = !mask.showMaskGraphic;
        // Disable immediately — Destroy is deferred to end-of-frame.
        mask.enabled = false;
        Object.Destroy(mask);

        if (hideMaskGraphic)
        {
            var graphic = viewport.GetComponent<Graphic>();
            if (graphic != null)
                graphic.enabled = false;
        }

        EnsureRectMask(viewport);
        return true;
    }

    private static void EnsureRectMask(GameObject viewport)
    {
        var rectMask = viewport.GetComponent<RectMask2D>();
        if (rectMask == null)
            rectMask = viewport.AddComponent<RectMask2D>();

        rectMask.enabled = true;

        var graphics = viewport.GetComponentsInChildren<MaskableGraphic>(true);
        for (var i = 0; i < graphics.Length; i++)
        {
            graphics[i].RecalculateClipping();
            graphics[i].SetVerticesDirty();
        }
    }
}
