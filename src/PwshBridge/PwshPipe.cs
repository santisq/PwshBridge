using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace PwshBridge;

public sealed class PwshPipe : IDisposable, IModuleAssemblyCleanup
{
    private readonly Process? _pwshProcess;

    private readonly NamedPipeConnectionInfo? _pipeInfo;

    private readonly Runspace _runspace;

    private readonly PowerShell _ps;

    private readonly InitialSessionState _iss = InitialSessionState.CreateDefault2();

    private bool _disposed;

    internal bool IsAlive =>
        !_disposed && _pwshProcess?.HasExited is not true;

    public PwshPipe(PSHost pSHost, string path, bool interactive)
    {
        string arguments = "-NoProfile -NoLogo";
        if (!interactive)
        {
            arguments += " -NonInteractive";
        }

        _pwshProcess = Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            FileName = path,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            LoadUserProfile = false,
            Arguments = arguments
        });

        _pipeInfo = new NamedPipeConnectionInfo(_pwshProcess.Id);
        _runspace = RunspaceFactory.CreateRunspace(pSHost, _pipeInfo);
        _runspace.Open();
        _ps = PowerShell.Create(_iss);
        _ps.Runspace = _runspace;
    }

    internal void Invoke(
        ScriptBlock scriptBlock,
        object[] arguments,
        PSCmdlet cmdlet)
    {
        cmdlet.WriteObject(
            AddScript(scriptBlock).
                AddArguments(arguments).
                _ps.Invoke(),
            enumerateCollection: true);

        if (_ps.HadErrors)
        {
            foreach (ErrorRecord error in _ps.Streams.Error)
            {
                cmdlet.WriteError(error);
            }
        }
    }

    private PwshPipe AddScript(ScriptBlock scriptBlock)
    {
        _ps.Commands.Clear();
        _ps.AddScript(script: scriptBlock.ToString(), useLocalScope: false);
        return this;
    }

    private PwshPipe AddArguments(object[] args)
    {
        foreach (object arg in args)
        {
            _ps.AddArgument(arg);
        }
        return this;
    }

    internal Process? GetProcess() => _pwshProcess;

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _ps?.Dispose();
        _runspace?.Dispose();
        _pwshProcess?.Kill();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void OnRemove(PSModuleInfo psModuleInfo) => Dispose();
}
