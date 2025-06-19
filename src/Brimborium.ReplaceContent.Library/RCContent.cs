namespace Brimborium.ReplaceContent;

/// <summary>
/// Represents content to be processed for placeholder replacement.
/// </summary>
public sealed class RCContent {
    /// <summary>
    /// Initializes a new instance of the <see cref="RCContent"/> class.
    /// </summary>
    /// <param name="identifier">The unique identifier for this content.</param>
    public RCContent(string identifier) {
        this.Identifier = identifier;
    }

    /// <summary>
    /// Gets the unique identifier for this content.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets or sets the file path of the content.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the target file path for the modified content.
    /// </summary>
    public string? NextFilePath { get; set; }

    /// <summary>
    /// Gets or sets the file type which determines comment syntax.
    /// </summary>
    public RCFileType? FileType { get; set; }

    /// <summary>
    /// Gets or sets the current content before processing.
    /// </summary>
    public StringSlice? CurrentContent { get; set; }

    /// <summary>
    /// Gets or sets the content after placeholder replacement.
    /// </summary>
    public StringSlice? NextContent { get; set; }

    /// <summary>
    /// Gets the parse result containing placeholder information.
    /// </summary>
    public RCParseResult ParseResult { get; } = new(new());

    /// <summary>
    /// Gets or sets a value indicating whether the content has been modified.
    /// </summary>
    public bool Modified { get; set; }
}
