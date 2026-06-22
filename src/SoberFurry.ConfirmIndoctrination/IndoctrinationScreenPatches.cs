using System;
using System.Collections.Generic;
using HarmonyLib;
using Lamb.UI;
using UnityEngine;

namespace SoberFurry.ConfirmIndoctrination;

/// <summary>
/// EXPERIMENTAL in-screen cancel (Mode = InsideScreen or Both). OFF by default.
///
/// It never calls RemoveAllListeners and never simulates the vanilla Accept button. It adds a
/// separate IMGUI "Cancel indoctrination" button; pressing it sets an abort flag and closes the
/// menu through the vanilla <see cref="UIMenuBase.Hide(bool)"/> path, while a prefix on
/// OnHideCompleted suppresses the recruit-finalise callback so the follower stays unrecruited.
///
/// This abort path could not be runtime-verified in this environment (no in-game input automation),
/// so it is fail-open and disabled by default; BeforeScreen is the guaranteed-safe mechanism.
/// </summary>
internal static class IndoctrinationScreenPatches
{
    internal static UIFollowerIndoctrinationMenuController? Current;
    private static readonly HashSet<int> Aborting = new();

    [HarmonyPatch(typeof(UIFollowerIndoctrinationMenuController), nameof(UIFollowerIndoctrinationMenuController.Show),
        new[] { typeof(Follower), typeof(OriginalFollowerLookData), typeof(bool) })]
    [HarmonyPostfix]
    private static void Show_Postfix(UIFollowerIndoctrinationMenuController __instance)
    {
        Current = __instance;
        if (ModeIncludesInside())
            InScreenCancelButton.Ensure();
        if (Plugin.Verbose) Plugin.Log.LogInfo("Indoctrination screen opened.");
    }

    [HarmonyPatch(typeof(UIFollowerIndoctrinationMenuController), "OnHideCompleted")]
    [HarmonyPrefix]
    private static bool OnHideCompleted_Prefix(UIFollowerIndoctrinationMenuController __instance)
    {
        int key = __instance.GetInstanceID();
        if (!Aborting.Remove(key))
        {
            if (Current == __instance) Current = null;
            return true; // normal completion -> vanilla finalises recruit
        }

        // Aborted: do the minimal safe cleanup vanilla would do, but skip the finalise callback.
        try
        {
            GameManager.InMenu = false;
            try { SimulationManager.UnPause(); } catch { }
            Plugin.Log.LogInfo($"ConfirmationCancelled stage=InsideScreen (experimental) menu={key}");
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"InsideScreen abort cleanup failed: {e}");
        }
        finally
        {
            if (Current == __instance) Current = null;
            try { UnityEngine.Object.Destroy(__instance.gameObject); } catch { }
        }
        return false; // suppress vanilla OnHideCompleted (which would finalise the recruit)
    }

    internal static void RequestAbort(UIFollowerIndoctrinationMenuController controller)
    {
        try
        {
            Aborting.Add(controller.GetInstanceID());
            controller.Hide(true);
        }
        catch (Exception e)
        {
            Aborting.Remove(controller.GetInstanceID());
            Plugin.Log.LogError($"InsideScreen abort failed (vanilla untouched): {e}");
        }
    }

    internal static bool ModeIncludesInside()
    {
        var m = Plugin.Cfg?.Mode.Value ?? WorkMode.BeforeScreen;
        return m == WorkMode.InsideScreen || m == WorkMode.Both;
    }

    internal static void Reset()
    {
        Aborting.Clear();
        Current = null;
    }

    /// <summary>IMGUI button shown only while the indoctrination screen is open.</summary>
    private sealed class InScreenCancelButton : MonoBehaviour
    {
        private static InScreenCancelButton? _instance;
        private GUIStyle? _style;

        public static void Ensure()
        {
            if (_instance != null) return;
            var go = new GameObject("SoberFurry.ConfirmIndoctrination.InScreenCancel");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            _instance = go.AddComponent<InScreenCancelButton>();
        }

        private void OnGUI()
        {
            if (Current == null || !ModeIncludesInside()) return;
            _style ??= new GUIStyle(GUI.skin.button) { fontSize = 18, padding = new RectOffset(14, 14, 10, 10) };
            float w = 320f, h = 52f;
            var rect = new Rect((Screen.width - w) / 2f, Screen.height - h - 24f, w, h);
            if (GUI.Button(rect, Localizer.Get("inside.cancel"), _style))
            {
                var c = Current;
                if (c != null) RequestAbort(c);
            }
        }
    }
}
