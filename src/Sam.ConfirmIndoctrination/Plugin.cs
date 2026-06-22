using System;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

#pragma warning disable CS0618
[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Sam.ConfirmIndoctrination;

[BepInPlugin(Guid, "Sam ConfirmIndoctrination", "1.1.0")]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.sam.cultofthelamb.confirmindoctrination";
    private const string LogPrefix = "Sam.ConfirmIndoctrination";

    internal static ManualLogSource Log = null!;
    internal static PluginConfig Cfg = null!;
    internal static ConfirmationOverlay Overlay = null!;
    internal static bool Verbose => Cfg != null && Cfg.VerboseLogging.Value;

    private Harmony _harmony = null!;

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"[{LogPrefix}] starting v1.1.0");
        try
        {
            Cfg = new PluginConfig(base.Config);
            Localizer.Init(Log);

            Overlay = ConfirmationOverlay.Create(Cfg);

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(FollowerRecruitPatches));
            _harmony.PatchAll(typeof(IndoctrinationScreenPatches));

            // Scene changes (pause -> main menu, biome transitions) must never leave a stale lock.
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += _ =>
            {
                FollowerRecruitPatches.ClearLocks();
                IndoctrinationScreenPatches.Reset();
                if (Overlay != null) Overlay.ForceClose();
            };

            int patched = 0;
            foreach (var _ in _harmony.GetPatchedMethods()) patched++;
            Log.LogInfo($"[{LogPrefix}] loaded. Mode={Cfg.Mode.Value}, " +
                        $"DefaultSelection={Cfg.DefaultSelection.Value}, Debounce={Cfg.InputDebounceMs.Value}ms, " +
                        $"patchedMethods={patched}.");
        }
        catch (Exception e)
        {
            Log.LogError($"[{LogPrefix}] FATAL during Awake (mod will be inert, vanilla game unaffected): {e}");
        }
    }

    private void OnDestroy()
    {
        try { _harmony?.UnpatchSelf(); } catch { }
    }
}
