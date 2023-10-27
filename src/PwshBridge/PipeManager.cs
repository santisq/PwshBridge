using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PwshBridge;

public sealed class PipeManager : IDisposable
{
    internal bool Disposed;

    private readonly ChildProcessManager _childProcessManager = new();

    public readonly Dictionary<string, PwshPipe> _pipemanager = new();

    internal PwshPipe this[string pipe] => _pipemanager[pipe];

    internal void AddPipe(string psVersion, PwshPipe pipe)
    {
        Process? process = pipe.GetProcess();
        if (process is not null)
        {
            _childProcessManager.AddProcess(process);
        }

        if (_pipemanager.TryGetValue(psVersion, out PwshPipe outpipe))
        {
            _pipemanager.Remove(psVersion);
            outpipe.Dispose();
        }

        _pipemanager.Add(psVersion, pipe);
    }

    public bool PipeIsAlive(string psVersion)
    {
        if (!_pipemanager.ContainsKey(psVersion))
        {
            return false;
        }

        return _pipemanager[psVersion].IsAlive;
    }

    private void Dispose(bool disposing)
    {
        if (Disposed || !disposing)
        {
            return;
        }

        _childProcessManager?.Dispose();
        foreach (PwshPipe pipe in _pipemanager.Values)
        {
            pipe.Dispose();
        }
        Disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
