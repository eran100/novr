using NOVR.PatchHelper;
using UnityEngine;
using UnityEngine.UI;

namespace NOVR.Patches.UI;

// World-space stencil Mask breaks TMP mission names; destroy it and use RectMask2D for clipping.
internal static class MissionSelectListItemPatch
{
    [PatchPrefix(typeof(MissionSelectListItem), "Awake")]
    private static void Awake(MissionSelectListItem __instance)
    {
        __instance.transform.localPosition = __instance.transform.localPosition with { z = 0 };
        var text = __instance.transform.Find("Text (TMP)");
        if (text != null)
            text.localPosition = text.localPosition with { z = 0 };

        EnsureRectMaskClipping(__instance.transform);
    }

    [PatchPostfix(typeof(MissionSelectListItem), "SetValue")]
    private static void SetValuePostfix(MissionSelectListItem __instance)
    {
        EnsureRectMaskClipping(__instance.transform);
    }

    private static void EnsureRectMaskClipping(Transform from)
    {
        var scrollRect = from.GetComponentInParent<ScrollRect>(true);
        var mask = from.GetComponentInParent<Mask>(true);
        var viewport = mask != null
            ? mask.gameObject
            : scrollRect != null && scrollRect.viewport != null
                ? scrollRect.viewport.gameObject
                : null;
        if (viewport == null) return;

        if (mask != null)
            Object.Destroy(mask);

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
