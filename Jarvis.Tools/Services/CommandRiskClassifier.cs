using Jarvis.Models;
using Jarvis.Security;

namespace Jarvis.Services;

public sealed class CommandRiskClassifier
{
    public SecurityRiskLevel Classify(PcCommand command)
    {
        return command.Action switch
        {
            PcControlAction.BrowserSearch => SecurityRiskLevel.Medium,
            PcControlAction.OpenApp => SecurityRiskLevel.Medium,
            PcControlAction.OpenWebsite => SecurityRiskLevel.Medium,
            PcControlAction.OpenFolder => SecurityRiskLevel.Medium,
            PcControlAction.OpenFile => SecurityRiskLevel.Medium,
            PcControlAction.VolumeUp => SecurityRiskLevel.Safe,
            PcControlAction.VolumeDown => SecurityRiskLevel.Safe,
            PcControlAction.ToggleMute => SecurityRiskLevel.Safe,
            PcControlAction.TakeScreenshot => SecurityRiskLevel.Dangerous,
            PcControlAction.Sleep => SecurityRiskLevel.Dangerous,
            PcControlAction.Shutdown => SecurityRiskLevel.Dangerous,
            PcControlAction.Restart => SecurityRiskLevel.Dangerous,
            _ => SecurityRiskLevel.Blocked
        };
    }
}
