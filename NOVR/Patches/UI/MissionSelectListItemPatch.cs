using NOVR.PatchHelper;
namespace NOVR.Patches.UI;

// Fixes invisible mission select items
internal static class MissionSelectListItemPatch
{
    [PatchPrefix(typeof(MissionSelectListItem), "Awake")]
    private static void Awake(MissionSelectListItem __instance)
    {
        // Items are instantiated at runtime with the prefab's layer (UI), which the
        // VR UI camera's culling mask does not render. UIRenderedCanvasBehavior
        // sweeps dynamic children onto the VrUi layer on a throttled cadence, but
        // zero the z offset and set the layer here too so rows are visible immediately.
        LayerHelper.SetLayerRecursive(__instance.transform, LayerHelper.GetVrUiLayer());
        __instance.transform.localPosition = __instance.transform.localPosition with { z = 0 };
        var text = __instance.transform.Find("Text (TMP)");
        text.localPosition = text.localPosition with { z = 0 };
    }
}
