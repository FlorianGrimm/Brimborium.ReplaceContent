using System.Linq;
using System.Management.Automation;

namespace Brimborium.ReplaceContent.PowershellCore;

/// <summary>
/// Update-ReplaceContent
/// </summary>
[Cmdlet(VerbsData.Update, Consts.ModulePrefix)]
//[OutputType(typeof(string))]
public sealed class UpdateCmdlet : PSCmdlet
{
    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }
}
