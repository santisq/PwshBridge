using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PwshBridge;

[Cmdlet(VerbsLifecycle.Invoke, "PSPipe")]
[OutputType(typeof(PSObject))]
[Alias("pspipe")]
public sealed class InvokePSPipeCommand : PSCmdlet
{
    [ThreadStatic]
    public static PipeManager? _manager;

    [Parameter(Position = 0, Mandatory = true)]
    public ScriptBlock ScriptBlock { get; set; } = null!;

    [Parameter(Position = 1)]
    [ArgumentCompleter(typeof(PwshTargetCompleter))]
    [ValidateNotNullOrEmpty]
    public string? PSVersion { get; set; }

    [Parameter]
    public object[] ArgumentList { get; set; } = Array.Empty<object>();

    [Parameter]
    public SwitchParameter Interative { get; set; }

    protected override void EndProcessing()
    {
        if (_manager is null or { Disposed: true })
        {
            _manager = new();
        }

        PSVersion ??= PwshAppsHelper.Get.First().Key;

        if (!_manager.PipeIsAlive(PSVersion))
        {
            PwshPipe pipe = new(
                (PSHost)GetVariableValue("Host"),
                PwshAppsHelper.Get[PSVersion].ResolvedPath,
                Interative.IsPresent);

            _manager.AddPipe(PSVersion, pipe);
        }

        _manager[PSVersion].Invoke(
            scriptBlock: ScriptBlock,
            arguments: ArgumentList,
            cmdlet: this);
    }
}
