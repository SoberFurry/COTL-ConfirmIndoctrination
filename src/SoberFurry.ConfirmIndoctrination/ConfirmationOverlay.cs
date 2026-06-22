using System;
using UnityEngine;

namespace SoberFurry.ConfirmIndoctrination;

/// <summary>
/// Self-contained IMGUI confirmation popup. It does not touch any vanilla UI element, so it can
/// never strip listeners or soft-lock the native menus. Cross-input:
///  - mouse: click the buttons (also works under Steam Remote Play, which streams the mouse);
///  - keyboard: Left/Right to move focus, Enter/Space to activate focus, Esc/Backspace = cancel;
///  - gamepad: South button activates focus, East button cancels, d-pad/stick moves focus.
/// Implements AwaitRelease + debounce so the very keypress that opened the popup cannot confirm it.
/// </summary>
internal sealed class ConfirmationOverlay : MonoBehaviour
{
    private enum Stage { Hidden, AwaitRelease, AwaitChoice }

    private Stage _stage = Stage.Hidden;
    private Action? _onAccept;
    private Action? _onCancel;
    private string _title = "";
    private bool _acceptFocused;
    private float _openedAt;
    private bool _seenRelease;

    private PluginConfig _config = null!;
    private GUIStyle? _boxStyle;
    private GUIStyle? _titleStyle;
    private GUIStyle? _btnStyle;
    private GUIStyle? _btnFocusStyle;

    public bool IsOpen => _stage != Stage.Hidden;

    public static ConfirmationOverlay Create(PluginConfig config)
    {
        var go = new GameObject("SoberFurry.ConfirmIndoctrination.Overlay");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        var overlay = go.AddComponent<ConfirmationOverlay>();
        overlay._config = config;
        return overlay;
    }

    public void Open(string title, Action onAccept, Action onCancel)
    {
        _title = title;
        _onAccept = onAccept;
        _onCancel = onCancel;
        _acceptFocused = _config.DefaultSelection.Value == DefaultChoice.Accept;
        _openedAt = Time.unscaledTime;
        _seenRelease = false;
        _stage = Stage.AwaitRelease;
    }

    public void ForceClose()
    {
        _stage = Stage.Hidden;
        _onAccept = null;
        _onCancel = null;
    }

    private bool AcceptHeld()
    {
        return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)
            || Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton0) || Input.GetKey(KeyCode.E);
    }

    private void Update()
    {
        if (_stage == Stage.Hidden) return;

        if (_stage == Stage.AwaitRelease)
        {
            // Wait until the interact input that opened us is fully released, then debounce.
            if (!AcceptHeld()) _seenRelease = true;
            float elapsedMs = (Time.unscaledTime - _openedAt) * 1000f;
            if (_seenRelease && elapsedMs >= _config.InputDebounceMs.Value)
                _stage = Stage.AwaitChoice;
            return;
        }

        // AwaitChoice
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Choose(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            _acceptFocused = !_acceptFocused;
        }
        float axis = 0f;
        try { axis = Input.GetAxisRaw("Horizontal"); } catch { }
        if (Mathf.Abs(axis) > 0.6f) _acceptFocused = axis > 0f ? false : true; // right=cancel(right side), left=accept(left side)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            Choose(_acceptFocused);
        }
    }

    private void Choose(bool accept)
    {
        var acc = _onAccept;
        var can = _onCancel;
        _stage = Stage.Hidden;
        _onAccept = null;
        _onCancel = null;
        try
        {
            if (accept) acc?.Invoke();
            else can?.Invoke();
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Confirmation callback threw: {e}");
        }
    }

    private void EnsureStyles()
    {
        if (_boxStyle != null) return;
        _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(24, 24, 24, 24) };
        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        _btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, padding = new RectOffset(16, 16, 12, 12) };
        _btnFocusStyle = new GUIStyle(_btnStyle) { fontStyle = FontStyle.Bold };
        _btnFocusStyle.normal.textColor = Color.yellow;
        _btnFocusStyle.hover.textColor = Color.yellow;
    }

    private void OnGUI()
    {
        if (_stage == Stage.Hidden) return;
        EnsureStyles();

        float w = Mathf.Min(560f, Screen.width * 0.6f);
        float h = 220f;
        var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

        // dim background so the popup reads clearly at any resolution / aspect.
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(rect, _boxStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(_title, _titleStyle);
        GUILayout.Space(24);

        bool enabled = _stage == Stage.AwaitChoice;
        GUI.enabled = enabled;
        GUILayout.BeginHorizontal();
        // Accept on the left, Cancel on the right.
        if (GUILayout.Button(Localizer.Get("confirm.accept"), _acceptFocused ? _btnFocusStyle : _btnStyle, GUILayout.Height(54)))
            Choose(true);
        GUILayout.Space(16);
        if (GUILayout.Button(Localizer.Get("confirm.cancel"), !_acceptFocused ? _btnFocusStyle : _btnStyle, GUILayout.Height(54)))
            Choose(false);
        GUILayout.EndHorizontal();
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
