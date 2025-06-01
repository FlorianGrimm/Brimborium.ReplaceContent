using System.Text;

namespace Brimborium.ReplaceContent;

public sealed class RCService {
    public RCContext NewContext() {
        return new RCContext();
    }

    public void Initialize(RCContext context) {
        {
            context.FileTypeByExtension[".ps1"] = new RCFileType("Powershell", "<#", "#>");
            context.FileTypeByExtension[".*"] = new RCFileType("Default", "/*", "*/");
        }
    }

    public void AddPlaceholder(
        RCContext context,
        string placeholder,
        string replacement) {
        context.Placeholders[placeholder] = replacement;
    }

    public void AddPlaceholderDictionary(
        RCContext context,
        Dictionary<string, string> placeholderReplacement) {
        foreach (var kvp in placeholderReplacement) {
            this.AddPlaceholder(context, kvp.Key, kvp.Value);
        }
    }

    public void AddPlaceholderFile(
        RCContext context,
        string filePath) {
        if (!filePath.EndsWith(".json")) {
            var placeholder = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var replacement = System.IO.File.ReadAllText(filePath);
            this.AddPlaceholder(context, placeholder, replacement);
        } else {
            var jsonContent = System.IO.File.ReadAllText(filePath);
            var placeholderReplacement = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            if (placeholderReplacement is not null) {
                this.AddPlaceholderDictionary(context, placeholderReplacement);
            } else {
                throw new System.InvalidOperationException($"Invalid JSON content in file: {filePath}");
            }
        }
    }

    public void AddPlaceholderDirectory(
        RCContext context,
        string directoryPath) {
        if (!System.IO.Path.IsPathFullyQualified(directoryPath)) {
            directoryPath = System.IO.Path.GetFullPath(directoryPath);
        }
        if (!System.IO.Directory.Exists(directoryPath)) {
            throw new System.IO.DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }
        {
            var files = System.IO.Directory.EnumerateFiles(directoryPath, "*.txt");
            foreach (var file in files) {
                this.AddPlaceholderFile(context, file);
            }
        }
        {
            var files = System.IO.Directory.EnumerateFiles(directoryPath, "*.json");
            foreach (var file in files) {
                this.AddPlaceholderFile(context, file);
            }
        }
    }

    public RCContent AddContentText(RCContext context, string identifier, string content) {
        var rcContent = new RCContent(identifier) {
            CurrentContent = content
        };
        context.Content.Add(rcContent.Identifier, rcContent);
        return rcContent;
    }

    public RCContent AddContentFile(RCContext context, string filePath) {
        var content = System.IO.File.ReadAllText(filePath);
        var rcContent = new RCContent(filePath) {
            FilePath = filePath,
            CurrentContent = content
        };
        context.Content.Add(rcContent.Identifier, rcContent);
        this.SetFileType(context, content);
        return rcContent;
    }

    public void AddContentDirectory(RCContext context, string directoryPath, HashSet<string>? fileExtensions) {
        System.IO.DirectoryInfo di = new(directoryPath);
        if (!di.Exists) {
            throw new System.IO.DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }
        foreach (var file in di.EnumerateFiles("*.*", System.IO.SearchOption.AllDirectories)) {
            if (file.Directory is { } directory
                && string.Equals(directory.Name, "Replacements", StringComparison.OrdinalIgnoreCase)) {
                continue; // Skip files in the Replacements directory
            }
            if (fileExtensions is null
                || fileExtensions.Contains(file.Extension)) {
                var content = this.AddContentFile(context, file.FullName);
                this.SetFileType(context, content);
            }
        }
    }

    public void AddFileType(RCContext context, string fileExtension, RCFileType fileType) {
        context.FileTypeByExtension[fileExtension] = fileType;
    }

    public RCFileType? SetFileType(RCContext context, RCContent content) {
        if (!(content.FilePath is { Length: > 0 } filePath)) {
            return null;
        }
        var fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        if (context.FileTypeByExtension.TryGetValue(fileExtension, out var fileType)) {
            return content.FileType = fileType;
        }
        // fallback to ".*" if specific extension not found
        if (context.FileTypeByExtension.TryGetValue(".*", out fileType)) {
            return content.FileType = fileType;
        }
        // not found
        {
            return null;
        }
    }

    public void Scan(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Scan(context, content);
        }
    }

    public void Scan(RCContext context, RCContent content) {
        if (!(content.CurrentContent is { Length: > 0 } currentContent)) { return; }
        if (!(content.FileType is { } fileType)) { return; }
        // TODO:
        content.ListPart.Add(new RCPart(RCPartType.ConstantText, currentContent, null));
#if false
        content.ListPart.Add(new RCPart(RCPartType.ConstantText, "start", null));
        content.ListPart.Add(new RCPart(RCPartType.PlaceholderStart, "/* <Placeholder Name> */", "Name"));
        content.ListPart.Add(new RCPart(RCPartType.PlaceholderContent, "old Content", "Name"));
        content.ListPart.Add(new RCPart(RCPartType.PlaceholderEnd, "/* </Placeholder Name> */", "Name"));
        content.ListPart.Add(new RCPart(RCPartType.ConstantText, "end", null));
#endif
    }

    public void Replace(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Replace(context, content);
            this.GenerateNextContent(context, content);
        }
    }

    public void Replace(RCContext context, RCContent content) {
        foreach (var part in content.ListPart) {
            if (part.PartType == RCPartType.PlaceholderContent
                && part.PlaceholderName is { Length: > 0 } placeholderName) {
                if (context.Placeholders.TryGetValue(placeholderName, out var replacement)) {
                    part.NextContent = replacement;
                }
            }
        }
    }

    public void GenerateNextContent(RCContext context, RCContent content) {
        bool hasPlaceholder = false;
        foreach (var part in content.ListPart) {
            if (part.NextContent is { } partNextContent) {
                if (string.Equals(content.CurrentContent, partNextContent, StringComparison.Ordinal)) {
                    // No changes here
                } else {
                    hasPlaceholder = true;
                    break;
                }
            }
        }

        if (!hasPlaceholder) {
            content.NextContent = null; // No changes, keep original content
            return;
        }

        var sbNextContent = new StringBuilder();
        {
            foreach (var part in content.ListPart) {
                if (part.NextContent is { Length: > 0 } partNextContent) {
                    sbNextContent.Append(partNextContent);
                } else {
                    sbNextContent.Append(part.OldContent);
                }
            }
        }

        {
            var nextContent = sbNextContent.ToString();
            if (string.Equals(nextContent, content.CurrentContent, StringComparison.Ordinal)) {
                nextContent = null; // No changes
            } else {
                content.NextContent = nextContent;
            }
        }
    }

    public void ShowDiff(RCContext context) {
        var sbDiff = new System.Text.StringBuilder();
        foreach (var content in context.Content.Values) {
            this.ShowDiff(context, content, sbDiff);
        }
    }

    public void ShowDiff(RCContext context, RCContent content, StringBuilder sbDiff) {
        foreach (var part in content.ListPart) {
            if (part.PartType == RCPartType.PlaceholderContent) {
                if (part.NextContent is { Length: > 0 } nextContent) {
                    if (string.Equals(part.OldContent, nextContent, StringComparison.Ordinal)) {
                        continue; // No change
                    }
                    // TODO: git diff style output
                    sbDiff.AppendLine($"Identifier: {content.Identifier}");
                    sbDiff.AppendLine($"Placeholder: {part.PlaceholderName}");
                    sbDiff.AppendLine($"Old Content: {part.OldContent}");
                    sbDiff.AppendLine($"New Content: {nextContent}");
                } else {
                    sbDiff.AppendLine($"Placeholder: {part.PlaceholderName} (no replacement found)");
                }
            }
        }
    }

    public void Write(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Write(context, content);
        }
    }

    public void Write(RCContext context, RCContent content) {

    }
}
