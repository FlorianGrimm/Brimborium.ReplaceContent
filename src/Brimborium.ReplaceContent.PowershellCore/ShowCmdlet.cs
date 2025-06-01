using System.Management.Automation;

namespace Brimborium.ReplaceContent.PowershellCore;

/// <summary>
/// Show-ReplaceContent
/// </summary>
[Cmdlet(VerbsCommon.Show, Consts.ModulePrefix)]
//[OutputType(typeof(string))]
public sealed class ShowCmdlet : PSCmdlet
{
    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }
}
