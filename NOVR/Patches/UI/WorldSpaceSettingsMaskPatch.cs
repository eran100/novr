using NOVR.PatchHelper;
using Rewired.UI.ControlMapper;
using UnityEngine;

namespace NOVR.Patches.UI;

/// <summary>
/// Stock settings/keybind scroll views use stencil Mask, which hides TMP under world-space MainCanvas.
/// </summary>
internal static class WorldSpaceSettingsMaskPatch
{
    [PatchPostfix(typeof(SettingsMenu), "OpenMenu")]
    private static void SettingsMenu_OpenMenu_Postfix(SettingsMenu __instance)
    {
        if (__instance == null) return;
        WorldSpaceUiMaskFix.ReplaceMasksWithRectMask2D(__instance.transform);
    }

    [PatchPostfix(typeof(GraphicsMenu), "Start")]
    private static void GraphicsMenu_Start_Postfix(GraphicsMenu __instance)
    {
        if (__instance == null) return;
        WorldSpaceUiMaskFix.ReplaceMasksWithRectMask2D(__instance.transform);
    }

    [PatchPostfix(typeof(ControlMapper), "Open")]
    private static void ControlMapper_Open_Postfix(ControlMapper __instance)
    {
        if (__instance == null) return;
        WorldSpaceUiMaskFix.ReplaceMasksWithRectMask2D(__instance.transform);
    }
}
