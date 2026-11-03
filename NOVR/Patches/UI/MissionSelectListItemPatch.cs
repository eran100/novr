using System;
using System.IO;
using System.Reflection;
using System.Text;
using NOVR.PatchHelper;
using UnityEngine;
using UnityEngine.UI;

namespace NOVR.Patches.UI;

// World-space stencil Mask breaks TMP mission names; replace with RectMask2D for clipping.
internal static class MissionSelectListItemPatch
{
    private static int _clipFixLogs;

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
        var mask = from.GetComponentInParent<Mask>(true);
        if (mask == null) return;

        var viewport = mask.gameObject;
        var maskWasEnabled = mask.enabled;
        if (maskWasEnabled)
            mask.enabled = false;

        var rectMask = viewport.GetComponent<RectMask2D>();
        var addedRectMask = false;
        if (rectMask == null)
        {
            rectMask = viewport.AddComponent<RectMask2D>();
            addedRectMask = true;
        }

        // #region agent log
        if (_clipFixLogs < 4)
        {
            _clipFixLogs++;
            AgentLog(
                "S1",
                "MissionSelectListItemPatch.EnsureRectMaskClipping",
                "clip-fix",
                $"{{\"maskWasEnabled\":{maskWasEnabled.ToString().ToLowerInvariant()},\"maskNowEnabled\":{mask.enabled.ToString().ToLowerInvariant()},\"addedRectMask\":{addedRectMask.ToString().ToLowerInvariant()},\"hasRectMask\":{(rectMask != null).ToString().ToLowerInvariant()},\"rectMaskEnabled\":{(rectMask != null && rectMask.enabled).ToString().ToLowerInvariant()},\"viewport\":\"{Escape(GetPath(viewport.transform))}\"}}");
        }
        // #endregion
    }

    // #region agent log
    private static void AgentLog(string hypothesisId, string location, string message, string dataJsonObject)
    {
        var payload =
            $"{{\"sessionId\":\"a69714\",\"runId\":\"scroll-fix\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"{Escape(location)}\",\"message\":\"{Escape(message)}\",\"data\":{dataJsonObject},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}";

        try
        {
            NOVRPlugin.LogSource?.LogMessage($"[NOVR-DBG] {message} {dataJsonObject}");
        }
        catch
        {
            // ignored
        }

        try
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(pluginDir))
                File.AppendAllText(Path.Combine(pluginDir, "debug-a69714.log"), payload + "\n");
        }
        catch
        {
            // ignored
        }
    }

    private static string Escape(string value) =>
        (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");

    private static string GetPath(Transform transform)
    {
        var sb = new StringBuilder();
        while (transform != null)
        {
            if (sb.Length > 0) sb.Insert(0, '/');
            sb.Insert(0, transform.name);
            transform = transform.parent;
        }

        return sb.ToString();
    }
    // #endregion
}
