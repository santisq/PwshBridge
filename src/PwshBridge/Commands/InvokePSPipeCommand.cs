using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PwshBridge;

[Cmdlet(VerbsLifecycle.Invoke, "PSPipe")]
[OutputType(typeof(PSObject))]
public sealed class InvokePSPipeCommand : PSCmdlet
{
    [ThreadStatic]
    internal static PwshPipe? _pipe;

    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock ScriptBlock { get; set; } = null!;

    [Parameter]
    public object[] ArgumentList { get; set; } = Array.Empty<object>();

    protected override void EndProcessing()
    {
        if (_pipe is null or { IsAlive: false })
        {
            _pipe = new((PSHost)GetVariableValue("Host"));
        }

        _pipe.Invoke(
            scriptBlock: ScriptBlock,
            arguments: ArgumentList,
            cmdlet: this);
    }
}
