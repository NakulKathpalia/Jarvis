using Jarvis.Models;

namespace Jarvis.Services;

public sealed class CommandSafetyService
{
    public CommandSafetyLevel GetSafetyLevel(PcControlAction action)
    {
        return action switch
        {
            PcControlAction.BrowserSearch => CommandSafetyLevel.Safe,
            PcControlAction.VolumeUp => CommandSafetyLevel.Safe,
            PcControlAction.VolumeDown => CommandSafetyLevel.Safe,
            PcControlAction.ToggleMute => CommandSafetyLevel.Safe,
            PcControlAction.OpenApp => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.OpenWebsite => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.OpenFolder => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.OpenFile => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.TakeScreenshot => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.Sleep => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.Shutdown => CommandSafetyLevel.ConfirmationRequired,
            PcControlAction.Restart => CommandSafetyLevel.ConfirmationRequired,
            _ => CommandSafetyLevel.Blocked
        };
    }
}
