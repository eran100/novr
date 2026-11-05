using NOVR.PatchHelper;
using UnityEngine;

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

        WorldSpaceUiMaskFix.EnsureRectMaskClipping(__instance.transform);
    }

    [PatchPostfix(typeof(MissionSelectListItem), "SetValue")]
    private static void SetValuePostfix(MissionSelectListItem __instance)
    {
        WorldSpaceUiMaskFix.EnsureRectMaskClipping(__instance.transform);
    }
}
