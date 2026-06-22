using BepInEx.Configuration;

namespace SoberFurry.ConfirmIndoctrination;

internal enum WorkMode
{
    /// <summary>Only the confirmation popup before the indoctrination screen opens.</summary>
    BeforeScreen,
    /// <summary>Only the experimental cancel button inside the indoctrination screen.</summary>
    InsideScreen,
    /// <summary>Both mechanisms.</summary>
    Both,
    /// <summary>Loaded but inert.</summary>
    Disabled
}

internal enum DefaultChoice
{
    Cancel,
    Accept
}

internal sealed class PluginConfig
{
    public readonly ConfigEntry<WorkMode> Mode;
    public readonly ConfigEntry<DefaultChoice> DefaultSelection;
    public readonly ConfigEntry<int> InputDebounceMs;
    public readonly ConfigEntry<bool> AllowCancelForTutorial;
    public readonly ConfigEntry<bool> AllowCancelForIntegrations;
    public readonly ConfigEntry<bool> VerboseLogging;

    public PluginConfig(ConfigFile cfg)
    {
        // Default is BeforeScreen (guaranteed-safe). InsideScreen abort is experimental and
        // fail-open; see README. Set Mode=Both to also enable the in-screen cancel button.
        Mode = cfg.Bind("General", "Mode", WorkMode.BeforeScreen,
            "BeforeScreen = confirm popup only (safe, recommended). InsideScreen = experimental in-screen cancel only. Both = both. Disabled = inert.");

        DefaultSelection = cfg.Bind("General", "DefaultSelection", DefaultChoice.Cancel,
            "Which button is focused by default in the confirmation popup.");

        InputDebounceMs = cfg.Bind("Input", "InputDebounceMs", 200,
            new ConfigDescription("Delay after the interact button is released before Accept becomes usable.",
                new AcceptableValueRange<int>(0, 1000)));

        AllowCancelForTutorial = cfg.Bind("Filters", "AllowCancelForTutorial", false,
            "If false, forced/tutorial recruits are never intercepted (story safety).");

        AllowCancelForIntegrations = cfg.Bind("Filters", "AllowCancelForIntegrations", false,
            "If false, Twitch/integration-created followers are never intercepted unless verified safe.");

        VerboseLogging = cfg.Bind("Diagnostics", "VerboseLogging", false,
            "Extra diagnostic logging.");
    }
}
