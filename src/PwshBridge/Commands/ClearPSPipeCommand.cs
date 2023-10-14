using System.Management.Automation;
using PwshBridge;

[Cmdlet(VerbsCommon.Clear, "PSPipe")]
public sealed class ClearPSPipeCommand : PSCmdlet
{
    protected override void EndProcessing() =>
        InvokePSPipeCommand._pipe?.Dispose();
}
