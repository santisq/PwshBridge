using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PwshBridge;

internal sealed class PwshTargetCompleter : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        foreach (KeyValuePair<string, PwshTarget> app in PwshAppsHelper.Get)
        {
            if (app.Key.StartsWith(wordToComplete, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return new CompletionResult(
                    completionText: app.Key,
                    listItemText: app.Value.CompletionText,
                    resultType: CompletionResultType.ParameterValue,
                    app.Value.ResolvedPath);
            }
        }
    }
}
