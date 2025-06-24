namespace Brimborium.ReplaceContent;

/// <summary>
/// Defines the types of parts that can be identified during content parsing.
/// </summary>
public enum RCPartType {
    /// <summary>
    /// Represents an error encountered during parsing.
    /// </summary>
    Error,

    /// <summary>
    /// Represents regular text content that is not part of a placeholder.
    /// </summary>
    ConstantText = 1,
    
    /// <summary>
    /// Represents the start tag of a placeholder.
    /// </summary>
    PlaceholderStart,
    
    /// <summary>
    /// Represents the end tag of a placeholder.
    /// </summary>
    PlaceholderEnd,
    
    /// <summary>
    /// Represents the content between placeholder start and end tags.
    /// </summary>
    PlaceholderContent,

    /// <summary>
    /// Represents the start tag of a placeholder.
    /// </summary>
    DefaultStart,

    /// <summary>
    /// Represents the end tag of a placeholder.
    /// </summary>
    DefaultEnd,

    /// <summary>
    /// Represents the content between placeholder start and end tags.
    /// </summary>
    DefaultContent,

    /// <summary>
    /// Represents the start tag of a placeholder.
    /// </summary>
    CustomizeStart,

    /// <summary>
    /// Represents the end tag of a placeholder.
    /// </summary>
    CustomizeEnd,

    /// <summary>
    /// Represents the content between placeholder start and end tags.
    /// </summary>
    CustomizeContent,
}
