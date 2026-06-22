using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SoberFurry.ConfirmIndoctrination;

/// <summary>
/// Intercepts the FIRST acceptance of a new waiting follower (FollowerRecruit interaction) and shows
/// a confirmation popup. Re-indoctrination uses Interaction_Reindoctrinate and is never touched here.
///
/// Design (matches the TZ):
///  - one-shot bypass keyed by follower id; after Accept the original interaction is re-invoked once;
///  - lock keyed by follower id prevents a second popup / co-op double-trigger;
///  - fail-open: if we cannot prove this is a normal first recruit, we let vanilla run.
/// </summary>
internal static class FollowerRecruitPatches
{
    private static readonly HashSet<int> Locked = new();
    private static readonly HashSet<int> Bypass = new();

    [HarmonyPatch(typeof(FollowerRecruit), nameof(FollowerRecruit.OnInteract))]
    [HarmonyPrefix]
    private static bool OnInteract_Prefix(FollowerRecruit __instance, StateMachine state)
        => Intercept(__instance, state, isPrimary: true);

    [HarmonyPatch(typeof(FollowerRecruit), nameof(FollowerRecruit.OnSecondaryInteract))]
    [HarmonyPrefix]
    private static bool OnSecondaryInteract_Prefix(FollowerRecruit __instance, StateMachine state)
        => Intercept(__instance, state, isPrimary: false);

    /// <returns>true = run vanilla, false = block vanilla (popup shown).</returns>
    private static bool Intercept(FollowerRecruit instance, StateMachine state, bool isPrimary)
    {
        try
        {
            var cfg = Plugin.Cfg;
            if (cfg == null || cfg.Mode.Value == WorkMode.Disabled) return true;
            if (cfg.Mode.Value == WorkMode.InsideScreen) return true; // BeforeScreen disabled

            int id = TryGetFollowerId(instance);
            if (id == int.MinValue)
            {
                Log("InvocationSkipped reason=NoFollowerId");
                return true; // fail-open
            }

            // One-shot bypass: the re-invocation after Accept runs vanilla exactly once.
            if (Bypass.Remove(id))
            {
                if (Plugin.Verbose) Log($"Bypass consumed for follower {id} -> vanilla runs once");
                return true;
            }

            // Already showing for this follower (or co-op second press): swallow.
            if (Locked.Contains(id))
            {
                Log($"LockRejected followerId={id}");
                return false;
            }

            if (!IsNormalFirstRecruit(instance, id, out string skipReason))
            {
                Log($"InvocationSkipped reason={skipReason} followerId={id}");
                return true; // fail-open / story safety
            }

            Locked.Add(id);
            Log($"ConfirmationShown followerId={id} source={(isPrimary ? "OnInteract" : "OnSecondaryInteract")} mode={cfg.Mode.Value}");

            Action onAccept = () =>
            {
                Locked.Remove(id);
                Bypass.Add(id);
                Log($"ConfirmationAccepted followerId={id}");
                try
                {
                    if (isPrimary) instance.OnInteract(state);
                    else instance.OnSecondaryInteract(state);
                }
                catch (Exception e)
                {
                    Bypass.Remove(id);
                    Plugin.Log.LogError($"Re-invocation of recruit failed for {id}: {e}");
                }
            };
            Action onCancel = () =>
            {
                Locked.Remove(id);
                Log($"ConfirmationCancelled followerId={id} stage=BeforeScreen");
            };

            // Prefer the native radial wheel (gamepad-friendly, Esc/cancel built in); fall back to overlay.
            if (!RadialConfirm.Show(instance.Follower, onAccept, onCancel))
                Plugin.Overlay.Open(Localizer.Get("confirm.title"), onAccept, onCancel);

            return false; // block vanilla; the confirmation decides.
        }
        catch (Exception e)
        {
            // Absolute fail-open: never break recruiting because of this mod.
            Plugin.Log.LogError($"Intercept failed, allowing vanilla: {e}");
            return true;
        }
    }

    private static int TryGetFollowerId(FollowerRecruit instance)
    {
        try
        {
            var info = instance?.Follower?.Brain?.Info;
            return info != null ? info.ID : int.MinValue;
        }
        catch { return int.MinValue; }
    }

    private static bool IsNormalFirstRecruit(FollowerRecruit instance, int id, out string reason)
    {
        reason = "";
        var cfg = Plugin.Cfg;

        // Twitch / integration followers carry a ViewerID.
        try
        {
            string viewer = instance.Follower.Brain.Info.ViewerID;
            if (!string.IsNullOrEmpty(viewer) && !cfg.AllowCancelForIntegrations.Value)
            {
                reason = "Integration";
                return false;
            }
        }
        catch { /* field may not exist on this build; ignore */ }

        // First / forced recruit (onboarding): don't risk the tutorial unless explicitly allowed.
        try
        {
            if (!cfg.AllowCancelForTutorial.Value && DataManager.Instance != null
                && DataManager.Instance.Followers != null && DataManager.Instance.Followers.Count == 0)
            {
                reason = "Tutorial";
                return false;
            }
        }
        catch { }

        return true;
    }

    /// <summary>Called from scene-change cleanup to drop any stale locks.</summary>
    internal static void ClearLocks()
    {
        int n = Locked.Count;
        Locked.Clear();
        Bypass.Clear();
        if (n > 0) Log($"SceneCleanup locksCleared={n}");
    }

    private static void Log(string msg)
    {
        if (Plugin.Verbose) Plugin.Log.LogInfo(msg);
    }
}
