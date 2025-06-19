namespace Brimborium.ReplaceContent;

/// <summary>
/// Represents the context for replace content operations, containing file types, placeholders, and content.
/// </summary>
public sealed class RCContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RCContext"/> class.
    /// </summary>
    public RCContext() {
    }
    
    /// <summary>
    /// Gets a dictionary mapping file extensions to their corresponding file types.
    /// The dictionary uses case-insensitive string comparison for keys.
    /// </summary>
    public Dictionary<string, RCFileType> FileTypeByExtension { get; } = new (StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Gets a dictionary of placeholder names and their replacement values.
    /// </summary>
    public RCReplacementDictionary DictReplacement { get; } = new ();
    
    /// <summary>
    /// Gets a dictionary of content items indexed by their identifiers.
    /// </summary>
    public Dictionary<string, RCContent> Content { get; } = new ();

    public bool TryGetReplacementValue(StringSlice placeholderName, [MaybeNullWhen(false)] out RCReplacementValue replacement) {
        return this.DictReplacement.TryGetValue(placeholderName, out replacement);
    }
}
