using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PwshBridge;

internal static class PwshAppsHelper
{
    [ThreadStatic]
    private static Dictionary<string, PwshTarget>? s_apps;

    public static Dictionary<string, PwshTarget> Get =>
        s_apps ??= GetApps();

    public static bool ContainsKey(string key) =>
        Get.ContainsKey(key);

    private static Dictionary<string, PwshTarget> GetApps()
    {
        using PowerShell ps = PowerShell.Create();
        return ps.AddScript(@"
            $set = [System.Collections.Generic.HashSet[string]]::new(
                [System.StringComparer]::InvariantCultureIgnoreCase)

            foreach ($app in Get-Command pwsh, powershell -CommandType Application) {
                $resolvedTarget = ($app.Source | Get-Item).ResolvedTarget

                if (-not $set.Add($resolvedTarget)) {
                    continue
                }

                $version = $app.Version
                if (-not $IsWindows) {
                    $version = [System.Reflection.AssemblyName]::GetAssemblyName(
                        [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName(
                            $resolvedTarget),
                            'System.Management.Automation.dll')).
                        Version
                }

                [pscustomobject]@{
                    CompletionText = '{0} ({1})' -f [System.IO.Path]::GetFileNameWithoutExtension($app.Name), $version
                    ResolvedPath   = $resolvedTarget
                }
            }", useLocalScope: false)
            .Invoke<PwshTarget>()
            .ToDictionary(e => e.CompletionText);
    }
}

internal record PwshTarget
{
    internal string CompletionText { get; set; } = null!;

    internal string ResolvedPath { get; set; } = null!;
}
