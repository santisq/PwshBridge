using System.Management.Automation;

namespace PwshBridge;

[Cmdlet(VerbsCommon.Clear, "PSPipe")]
public sealed class ClearPSPipeCommand : PSCmdlet
{
    protected override void EndProcessing() =>
        InvokePSPipeCommand._manager?.Dispose();
}
