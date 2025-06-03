using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;

namespace Brimborium.ReplaceContent.PowershellCore;

/// <summary>
/// Update-ReplaceContent
/// </summary>
[Cmdlet(VerbsData.Update, Consts.ModulePrefix)]
public sealed class UpdateCmdlet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Directory { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? File { get; set; }

    [Parameter(Mandatory = false, Position = 2)]
    public string ReplacementsDirectory { get; set; } = "Replacements";

    [Parameter(Mandatory = false, Position = 3)]
    public string[]? FileExtensions { get; set; }

    [Parameter(Mandatory = false, Position = 4)]
    public Dictionary<string, RCFileType> FileType = new();

    [Parameter(Mandatory = false, Position = 5)]
    public Dictionary<string, string> Replacements = new();

    protected override void BeginProcessing() {
        base.BeginProcessing();
        var replaceContentService = new RCService();
        var context = replaceContentService.NewContext();

        if (this.FileType is { Count: > 0 } appFileType) {
            foreach (var kvp in appFileType) {
                context.FileTypeByExtension[kvp.Key] = kvp.Value;
            }
        } else {
            replaceContentService.InitializeFileTypeByExtension(context);
        }

        replaceContentService.AddPlaceholderDirectory(context, this.ReplacementsDirectory, true);

        if (this.File is { Length: > 0 } filePath) {
            replaceContentService.AddContentFile(context, filePath);
        }

        if (this.Directory is { Length: > 0 } directoryPath) {
            if (this.FileExtensions is { Length: > 0 } fileExtensions) {
                var fileExtensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                replaceContentService.AddContentDirectory(context, directoryPath, fileExtensionsSet);
            } else {
                replaceContentService.AddContentDirectory(context, directoryPath, null);
            }
        }

        replaceContentService.Scan(context);
        replaceContentService.Replace(context);
        replaceContentService.Write(context);
    }
}
