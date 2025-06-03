using Brimborium.Text;

namespace Brimborium.ReplaceContent;

/// <summary>
/// Provides services for replacing content between placeholder tags in files.
/// </summary>
public sealed class RCService {
    private static readonly char[] _CrLf = new char[] { '\r', '\n' };

    /// <summary>
    /// Creates a new context for replace content operations.
    /// </summary>
    /// <returns>A new <see cref="RCContext"/> instance.</returns>
    public RCContext NewContext() {
        return new RCContext();
    }

    /// <summary>
    /// Initializes the file type mappings by file extension in the given context.
    /// </summary>
    /// <param name="context">The context to initialize.</param>
    public void InitializeFileTypeByExtension(RCContext context) {
        context.FileTypeByExtension[".ps1"] = new RCFileType("Powershell", "<#", "#>");
        context.FileTypeByExtension[".js"] = new RCFileType("Javascript", "/*", "*/");
        context.FileTypeByExtension[".jsx"] = new RCFileType("Javascript", "/*", "*/");
        context.FileTypeByExtension[".ts"] = new RCFileType("Typescript", "/*", "*/");
        context.FileTypeByExtension[".tsx"] = new RCFileType("Typescript", "/*", "*/");
        context.FileTypeByExtension[".cs"] = new RCFileType("c#", "/*", "*/");
        context.FileTypeByExtension[".sql"] = new RCFileType("SQL", "/*", "*/");
        context.FileTypeByExtension[".html"] = new RCFileType("HTML", "<!--", "-->");
        context.FileTypeByExtension[".*"] = new RCFileType("Default", "/*", "*/");
    }

    /// <summary>
    /// Adds a placeholder and its replacement to the context.
    /// </summary>
    /// <param name="context">The context to add the placeholder to.</param>
    /// <param name="placeholder">The placeholder name.</param>
    /// <param name="replacement">The replacement text.</param>
    public void AddPlaceholder(
        RCContext context,
        string placeholder,
        string replacement) {
        context.Placeholders[placeholder] = replacement;
    }

    /// <summary>
    /// Adds multiple placeholders and their replacements to the context.
    /// </summary>
    /// <param name="context">The context to add the placeholders to.</param>
    /// <param name="placeholderReplacement">A dictionary of placeholder names and their replacements.</param>
    public void AddPlaceholderDictionary(
        RCContext context,
        Dictionary<string, string> placeholderReplacement) {
        foreach (var kvp in placeholderReplacement) {
            this.AddPlaceholder(context, kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Adds placeholders from a file to the context.
    /// </summary>
    /// <param name="context">The context to add the placeholders to.</param>
    /// <param name="filePath">The path to the file containing placeholder data.</param>
    /// <remarks>
    /// If the file is a JSON file, it's expected to contain a dictionary of placeholder names and replacements.
    /// Otherwise, the file name (without extension) is used as the placeholder name and the file content as the replacement.
    /// </remarks>
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

    /// <summary>
    /// Adds placeholders from all .txt and .json files in a directory to the context.
    /// </summary>
    /// <param name="context">The context to add the placeholders to.</param>
    /// <param name="directoryPath">The path to the directory containing placeholder files.</param>
    /// <param name="optional">If true, no exception is thrown if the directory doesn't exist.</param>
    /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory doesn't exist and <paramref name="optional"/> is false.</exception>
    public void AddPlaceholderDirectory(
        RCContext context,
        string directoryPath,
        bool optional) {
        if (!System.IO.Path.IsPathFullyQualified(directoryPath)) {
            directoryPath = System.IO.Path.GetFullPath(directoryPath);
        }
        if (!System.IO.Directory.Exists(directoryPath)) {
            if (optional) {
                return; // Optional directory, do nothing
            }
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

    /// <summary>
    /// Adds text content to the context with the specified identifier.
    /// </summary>
    /// <param name="context">The context to add the content to.</param>
    /// <param name="identifier">The identifier for the content.</param>
    /// <param name="content">The text content.</param>
    /// <returns>The created <see cref="RCContent"/> instance.</returns>
    public RCContent AddContentText(RCContext context, string identifier, string content) {
        var rcContent = new RCContent(identifier) {
            CurrentContent = content
        };
        context.Content.Add(rcContent.Identifier, rcContent);
        return rcContent;
    }

    /// <summary>
    /// Adds file content to the context.
    /// </summary>
    /// <param name="context">The context to add the content to.</param>
    /// <param name="filePath">The path to the file to add.</param>
    /// <returns>The created <see cref="RCContent"/> instance.</returns>
    public RCContent AddContentFile(RCContext context, string filePath) {
        var content = System.IO.File.ReadAllText(filePath);
        var rcContent = new RCContent(filePath) {
            FilePath = filePath,
            CurrentContent = content
        };
        context.Content.Add(rcContent.Identifier, rcContent);
        this.SetFileType(context, rcContent);
        return rcContent;
    }

    /// <summary>
    /// Adds all files in a directory to the context.
    /// </summary>
    /// <param name="context">The context to add the content to.</param>
    /// <param name="directoryPath">The path to the directory containing files to add.</param>
    /// <param name="fileExtensions">Optional set of file extensions to filter by. If null, all files are included.</param>
    /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory doesn't exist.</exception>
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

    /// <summary>
    /// Adds a file type mapping to the context.
    /// </summary>
    /// <param name="context">The context to add the file type to.</param>
    /// <param name="fileExtension">The file extension to map.</param>
    /// <param name="fileType">The file type to associate with the extension.</param>
    public void AddFileType(RCContext context, string fileExtension, RCFileType fileType) {
        context.FileTypeByExtension[fileExtension] = fileType;
    }

    /// <summary>
    /// Sets the file type for a content item based on its file extension.
    /// </summary>
    /// <param name="context">The context containing file type mappings.</param>
    /// <param name="content">The content to set the file type for.</param>
    /// <returns>The file type that was set, or null if no matching file type was found.</returns>
    public RCFileType? SetFileType(RCContext context, RCContent content) {
        if (!((content.FilePath ?? content.Identifier) is { Length: > 0 } filePath)) {
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

    /// <summary>
    /// Scans all content in the context for placeholders.
    /// </summary>
    /// <param name="context">The context containing content to scan.</param>
    public void Scan(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Scan(context, content);
        }
    }

    /// <summary>
    /// Scans a specific content item for placeholders.
    /// </summary>
    /// <param name="context">The context containing file type information.</param>
    /// <param name="content">The content to scan.</param>
    /// <returns>An error part if scanning failed, or null if successful.</returns>
    public RCPart? Scan(RCContext context, RCContent content) {
        if (!(content.CurrentContent is { Length: > 0 } currentContent)) {
            return default;
        }
        if (!(content.FileType is { } fileType)) {
            return new RCPart(RCPartType.Error, string.Empty, "Unknown FileType", null, null);
        }
        if (!(fileType.CommentStart is { Length: > 0 } commentStart)) {
            return new RCPart(RCPartType.Error, string.Empty, "FileType.CommentStart is empty.", null, null);
        }
        if (!(fileType.CommentEnd is { Length: > 0 } commentEnd)) {
            return new RCPart(RCPartType.Error, string.Empty, "FileType.CommentEnd is empty.", null, null);
        }

        var parseResult = RCParser.Parse(content.CurrentContent, commentStart, commentEnd);
        if (parseResult.ContainsError(out var error)) {
            return error;
        }

        {
            content.ParseResult.ListPart.AddRange(parseResult.ListPart);
            return default;
        }
    }

    /// <summary>
    /// Replaces placeholders in all content in the context.
    /// </summary>
    /// <param name="context">The context containing content and placeholders.</param>
    public void Replace(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Replace(context, content);
            this.GenerateNextContent(context, content);
        }
    }

    /// <summary>
    /// Replaces placeholders in a specific content item.
    /// </summary>
    /// <param name="context">The context containing placeholders.</param>
    /// <param name="content">The content to process.</param>
    public void Replace(RCContext context, RCContent content) {
        if (content.ParseResult.ContainsError(out var error)) {
            // TODO: handle error
        } else {
            var listPart = content.ParseResult.ListPart;
            int index = 0;
            while (index < listPart.Count) {
                var part = listPart[index];
                if (part.PartType == RCPartType.ConstantText) {
                    index++;
                    continue;
                } else if ((part.PartType == RCPartType.PlaceholderStart)
                    && ((index + 2) < listPart.Count)) {
                    var partContent = listPart[index + 1];
                    var partEnd = listPart[index + 2];
                    if (partContent.PartType == RCPartType.PlaceholderContent
                        && partEnd.PartType == RCPartType.PlaceholderEnd) {

                        if (part.PlaceholderName is { Length: > 0 } placeholderName) {
                            if (context.Placeholders.TryGetValue(placeholderName, out var replacement)) {
                                var oldContentSlice = partContent.OldContent.AsStringSlice();
                                var oldContentEndsWithNewline = oldContentSlice.TrimEnd(_CrLf).SubstringBetweenEndAndEnd(oldContentSlice);
                                var replacementSlice = replacement.AsStringSlice();
                                if (part.Indentation is { Length: > 0 } indentation) {
                                    var result = NewlineTokenizer.Instance.Tokenize(replacementSlice);

                                    StringSliceBuilder builder = new StringSliceBuilder();
                                    bool lastWasNewline = false;
                                    foreach (var token in result.ListTokens) {
                                        if (token.Kind == NewlineToken.Word) {
                                            builder.Append(indentation);
                                            builder.Append(token.Text);
                                            lastWasNewline = false;
                                        } else {
                                            builder.Append(token.Text);
                                            lastWasNewline = true;
                                        }
                                    }
                                    if ((!oldContentEndsWithNewline.IsEmpty) && !lastWasNewline) {
                                        builder.Append(oldContentEndsWithNewline);
                                    }
                                    partContent.NextContent = builder.ToString();
                                } else {
                                    if (!oldContentEndsWithNewline.IsEmpty) {
                                        if (replacementSlice.TrimEnd(_CrLf).SubstringBetweenEndAndEnd(replacementSlice).IsEmpty) {
                                            // no newline at the end of replacement
                                            replacement = replacement + oldContentEndsWithNewline;
                                        }
                                    }
                                    partContent.NextContent = replacement;
                                }
                                if (!content.Modified) {
                                    content.Modified = string.Equals(partContent.OldContent, partContent.NextContent, StringComparison.Ordinal);
                                }
                            }
                        }

                        index += 3;
                        continue;
                    } else {
                        index++;
                        continue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates the next content for a content item after placeholder replacement.
    /// </summary>
    /// <param name="context">The context containing the content.</param>
    /// <param name="content">The content to generate next content for.</param>
    public void GenerateNextContent(RCContext context, RCContent content) {
        bool hasPlaceholder = content.Modified;
        if (!hasPlaceholder) {
            foreach (var part in content.ParseResult.ListPart) {
                if (part.NextContent is { } partNextContent) {
                    if (string.Equals(content.CurrentContent, partNextContent, StringComparison.Ordinal)) {
                        // No changes here
                    } else {
                        hasPlaceholder = true;
                        break;
                    }
                }
            }
        }

        if (!hasPlaceholder) {
            content.NextContent = null; // No changes, keep original content
            return;
        }

        {
            string nextContent;
            {
                var sbNextContent = new StringBuilder();

                foreach (var part in content.ParseResult.ListPart) {
                    if (part.NextContent is { Length: > 0 } partNextContent) {
                        sbNextContent.Append(partNextContent);
                    } else {
                        sbNextContent.Append(part.OldContent);
                    }
                }
                nextContent = sbNextContent.ToString();
            }

            if (string.Equals(nextContent, content.CurrentContent, StringComparison.Ordinal)) {
                content.NextContent = null; // No changes
                content.Modified = false;
            } else {
                content.NextContent = nextContent;
                content.Modified = true;
            }
        }
    }

    /// <summary>
    /// Displays differences between original and replaced content.
    /// </summary>
    /// <param name="context">The context containing content with replacements.</param>
    public void ShowDiff(RCContext context) {
        var sbDiff = new System.Text.StringBuilder();
        foreach (var content in context.Content.Values) {
            this.ShowDiff(context, content, sbDiff);
        }
        System.Console.WriteLine(sbDiff.ToString());
    }

    /// <summary>
    /// Displays differences for a specific content item.
    /// </summary>
    /// <param name="context">The context containing the content.</param>
    /// <param name="content">The content to show differences for.</param>
    /// <param name="sbDiff">The StringBuilder to append differences to.</param>
    public void ShowDiff(RCContext context, RCContent content, StringBuilder sbDiff) {
        if (!content.Modified) { return; }
        foreach (var part in content.ParseResult.ListPart) {
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

    /// <summary>
    /// Gets a list of content items that have been modified and creates temporary files for them.
    /// </summary>
    /// <param name="context">The context containing content with replacements.</param>
    /// <returns>A tuple containing the list of modified content items and a flag indicating if any operations were not possible.</returns>
    public (List<RCContent> result, bool containsNotPossible) GetDiffFile(RCContext context) {
        List<RCContent> result = new();
        bool containsNotPossible = false;
        foreach (var content in context.Content.Values) {
            var resultGetDiffFile = this.GetDiffFile(context, content);
            if (resultGetDiffFile == RCresultBool.True) {
                result.Add(content);
            } else if (resultGetDiffFile == RCresultBool.NotPossible) {
                containsNotPossible = true;
            }
        }
        return (result, containsNotPossible);
    }

    /// <summary>
    /// Creates a temporary file with the replaced content for a specific content item.
    /// </summary>
    /// <param name="context">The context containing the content.</param>
    /// <param name="content">The content to process.</param>
    /// <returns>A <see cref="RCresultBool"/> indicating success or failure.</returns>
    public RCresultBool GetDiffFile(RCContext context, RCContent content) {
        if (!(content.FilePath is { Length: > 0 } filePath)) { return RCresultBool.NotPossible; }
        content.NextFilePath = filePath + ".temp";
        if (content.Modified && content.NextContent is { Length: > 0 } nextContent) {
            System.IO.File.WriteAllText(content.NextFilePath, nextContent);
            return RCresultBool.True;
        } else {
            if (System.IO.File.Exists(content.NextFilePath)) {
                try {
                    System.IO.File.Delete(content.NextFilePath);
                } catch {
                }
            }
            return RCresultBool.False;
        }
    }

    /// <summary>
    /// Writes the modified content back to the original files.
    /// </summary>
    /// <param name="context">The context containing content to write.</param>
    public void Write(RCContext context) {
        foreach (var content in context.Content.Values) {
            this.Write(context, content);
        }
    }

    /// <summary>
    /// Writes the modified content back to the original file.
    /// </summary>
    /// <param name="context">The context containing content to write.</param>
    /// <param name="content">The content to write.</param>
    public void Write(RCContext context, RCContent content) {
        if (!(content.FilePath is { Length: > 0 } filePath)) {
            return;
        }
        if (content.Modified) {
            if (content.NextContent is { Length: > 0 } nextContent) {
                System.IO.File.WriteAllText(filePath, nextContent);
            }
        }
    }
}
public enum RCresultBool {
    NotPossible = 0,
    True = 1,
    False = 2
}
