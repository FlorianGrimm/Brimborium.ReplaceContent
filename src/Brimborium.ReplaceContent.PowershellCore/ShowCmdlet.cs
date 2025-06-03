using System.Management.Automation;
using System.Security.Cryptography;

namespace Brimborium.ReplaceContent.PowershellCore;

/// <summary>
/// Show-ReplaceContent
/// </summary>
[Cmdlet(VerbsCommon.Show, Consts.ModulePrefix)]
//[OutputType(typeof(string))]
public sealed class ShowCmdlet : PSCmdlet {
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

    [Parameter(Mandatory = false, Position = 6)]
    public SwitchParameter PassThou = new();

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
        var (listDiff, _) = replaceContentService.GetDiffFile(context);

        var useDiffEngine = !this.PassThou.ToBool() && !DiffEngine.DiffRunner.Disabled;
        if (useDiffEngine) {
            if (listDiff.Count == 0) {
                return;
            }
            foreach (var diff in listDiff) {
                if (diff.NextFilePath is null || diff.FilePath is null) {
                    continue;
                }
                var launchResult = DiffEngine.DiffRunner.LaunchForText(
                    tempFile: diff.NextFilePath,
                    targetFile: diff.FilePath);
                switch (launchResult) {
                    case DiffEngine.LaunchResult.NoEmptyFileForExtension:
                        break;
                    case DiffEngine.LaunchResult.AlreadyRunningAndSupportsRefresh:
                        break;
                    case DiffEngine.LaunchResult.StartedNewInstance:
                        break;
                    case DiffEngine.LaunchResult.TooManyRunningDiffTools:
                        return;
                    case DiffEngine.LaunchResult.NoDiffToolFound:
                        return;
                    case DiffEngine.LaunchResult.Disabled:
                        return;
                    default:
                        break;
                }
            }
        } else {
            this.WriteObject(listDiff, true);
        }
    }
}
