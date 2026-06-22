#if COTL_API
using System;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using COTL_API.CustomSettings;
using Lamb.UI;

namespace SoberFurry.ConfirmIndoctrination;

/// <summary>
/// OPTIONAL integration with COTL_API: if installed, the mod's settings appear in the in-game "Mods"
/// settings tab. Without COTL_API this is never called and the .cfg file is used. Compiled only when
/// COTL_API is available at build time.
/// </summary>
internal static class CotlApiIntegration
{
    private const string ApiGuid = "io.github.xhayper.COTL_API";

    public static void TryRegisterSettings()
    {
        try
        {
            if (!Chainloader.PluginInfos.ContainsKey(ApiGuid)) return;
            Register();
            Plugin.Log.LogInfo("Settings registered in the in-game COTL_API \"Mods\" tab.");
        }
        catch (Exception e)
        {
            Plugin.Log.LogWarning($"COTL_API settings registration failed (the .cfg still works): {e.Message}");
        }
    }

    private static void Register()
    {
        var c = Plugin.Cfg;
        const string cat = "Confirm Indoctrination";
        AddEnumDropdown(cat, "Mode", c.Mode);
        AddEnumDropdown(cat, "Default selection", c.DefaultSelection);
        CustomSettingsManager.AddBepInExConfig(cat, "Input debounce (ms)", c.InputDebounceMs, 10, MMSlider.ValueDisplayFormat.RawValue);
        CustomSettingsManager.AddBepInExConfig(cat, "Allow cancel for tutorial", c.AllowCancelForTutorial);
        CustomSettingsManager.AddBepInExConfig(cat, "Allow cancel for integrations", c.AllowCancelForIntegrations);
        CustomSettingsManager.AddBepInExConfig(cat, "Verbose logging", c.VerboseLogging);
    }

    private static void AddEnumDropdown<T>(string category, string name, ConfigEntry<T> entry) where T : struct, Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var names = Enum.GetNames(typeof(T));
        CustomSettingsManager.AddDropdown(category, name, entry.Value.ToString(), names, idx =>
        {
            if (idx >= 0 && idx < values.Length) entry.Value = values[idx];
        });
    }
}
#endif
