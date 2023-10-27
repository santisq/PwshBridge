using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PwshBridge;

[Cmdlet(
    VerbsLifecycle.Invoke, "PSPipe",
    DefaultParameterSetName = _scriptBlockOnlySet)]
[OutputType(typeof(PSObject))]
[Alias("pspipe")]
public sealed class InvokePSPipeCommand : PSCmdlet
{
    private const string _scriptBlockOnlySet = "ScriptBlockOnly";

    private const string _withPSVersionSet = "PSVersion";

    [ThreadStatic]
    internal static PwshPipe? _pipe;

    [Parameter(
        ParameterSetName = _withPSVersionSet,
        Position = 0)]
    [PwshTargetTransformation]
    [ArgumentCompleter(typeof(PwshTargetCompleter))]
    [ValidateNotNullOrEmpty]
    public string? PSVersion { get; set; }

    [Parameter(
        ParameterSetName = _scriptBlockOnlySet,
        Mandatory = true,
        Position = 0)]
    [Parameter(
        ParameterSetName = _withPSVersionSet,
        Mandatory = true,
        Position = 1)]
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
