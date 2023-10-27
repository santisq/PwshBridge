using System.Management.Automation;

namespace PwshBridge;

public sealed class PwshTargetTransformation : ArgumentTransformationAttribute
{
    public override object Transform(
        EngineIntrinsics engineIntrinsics,
        object inputData)
    {
        string psver = (string)inputData;
        if (!PwshAppsHelper.ContainsKey(psver))
        {
            throw new ArgumentTransformationMetadataException(
                $"PowerShell version could not be determined for input '{psver}'.");
        }

        return PwshAppsHelper.Get[psver].ResolvedPath;
    }
}
