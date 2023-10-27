using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace PwshBridge;

public sealed class PwshTargetCompleter : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        StringBuilder sb = new();
        foreach (KeyValuePair<string, PwshTarget> app in PwshAppsHelper.Get)
        {
            if (app.Key.StartsWith(wordToComplete, StringComparison.InvariantCultureIgnoreCase))
            {
                string completionText = sb
                    .Append("'")
                    .Append(CodeGeneration.EscapeSingleQuotedStringContent(app.Key))
                    .Append("'")
                    .ToString();

                yield return new CompletionResult(
                    completionText: completionText,
                    listItemText: app.Key,
                    resultType: CompletionResultType.ParameterValue,
                    app.Value.ResolvedPath);
            }

            sb.Clear();
        }
    }
}
