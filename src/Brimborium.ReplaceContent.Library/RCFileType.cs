namespace Brimborium.ReplaceContent;

/// <summary>
/// Represents a file type with specific comment syntax for placeholder identification.
/// </summary>
public class RCFileType {
    /// <summary>
    /// Initializes a new instance of the <see cref="RCFileType"/> class.
    /// </summary>
    /// <param name="name">The name of the file type (e.g., "C#", "SQL", "HTML").</param>
    /// <param name="commentStart">The string that marks the start of a comment in this file type.</param>
    /// <param name="commentEnd">The string that marks the end of a comment in this file type.</param>
    public RCFileType(string name, string commentStart, string commentEnd) {
        this.Name = name;
        CommentStart = commentStart;
        CommentEnd = commentEnd;
    }

    /// <summary>
    /// Gets the name of the file type.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the string that marks the start of a comment in this file type.
    /// </summary>
    public string CommentStart { get; }
    
    /// <summary>
    /// Gets the string that marks the end of a comment in this file type.
    /// </summary>
    public string CommentEnd { get; }
}
