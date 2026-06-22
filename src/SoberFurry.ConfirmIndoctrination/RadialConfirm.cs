using System;
using System.Collections.Generic;
using Lamb.UI;
using Lamb.UI.FollowerInteractionWheel;
using src.Extensions;

namespace SoberFurry.ConfirmIndoctrination;

/// <summary>
/// Shows the confirmation as the game's NATIVE radial command wheel (gamepad-friendly, with built-in
/// cancel on Escape / gamepad cancel button). Two slices: Accept and Cancel.
///
/// Falls back to the IMGUI overlay if the wheel template is not available.
/// </summary>
internal static class RadialConfirm
{
    private const int AcceptId = 930001;
    private const int CancelId = 930002;

    private sealed class ConfirmItem : CommandItem
    {
        public string Title = "";
        public string Desc = "";
        public string Icon = "";
        public override string GetTitle(Follower follower) => Title;
        public override string GetDescription(Follower follower) => Desc;
        public override string GetIcon() => Icon;
    }

    public static bool Show(Follower recruit, Action onAccept, Action onCancel)
    {
        try
        {
            var ui = MonoSingleton<UIManager>.Instance;
            if (ui == null || ui.FollowerInteractionWheelTemplate == null || recruit == null)
                return false;

            var wheel = ui.FollowerInteractionWheelTemplate.Instantiate();
            if (wheel == null) return false;

            var items = new List<CommandItem>
            {
                new ConfirmItem
                {
                    Command = (FollowerCommands)AcceptId,
                    Title = Localizer.Get("confirm.accept"),
                    Desc = Localizer.Get("confirm.title"),
                    Icon = FontImageNames.IconForCommand(FollowerCommands.AreYouSureYes)
                },
                new ConfirmItem
                {
                    Command = (FollowerCommands)CancelId,
                    Title = Localizer.Get("confirm.cancel"),
                    Desc = Localizer.Get("confirm.title"),
                    Icon = FontImageNames.IconForCommand(FollowerCommands.AreYouSureNo)
                }
            };

            bool done = false;
            void Finish(bool accept)
            {
                if (done) return;
                done = true;
                try { if (accept) onAccept?.Invoke(); else onCancel?.Invoke(); }
                catch (Exception e) { Plugin.Log.LogError($"Confirm callback failed: {e}"); }
            }

            wheel.OnItemChosen = (Action<FollowerCommands[]>)Delegate.Combine(
                wheel.OnItemChosen, new Action<FollowerCommands[]>(chosen =>
                {
                    int id = (chosen != null && chosen.Length > 0) ? (int)chosen[0] : CancelId;
                    Finish(id == AcceptId);
                }));
            wheel.OnCancel = (Action)Delegate.Combine(wheel.OnCancel, new Action(() => Finish(false)));

            wheel.Show(recruit, items, instant: false, cancellable: true);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"RadialConfirm failed, will fall back to overlay: {e}");
            return false;
        }
    }
}
