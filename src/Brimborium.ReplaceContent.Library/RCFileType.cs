namespace Brimborium.ReplaceContent;

public class RCFileType {
    public RCFileType(string name, string commentStart, string commentEnd) {
        this.Name = name;
        CommentStart = commentStart;
        CommentEnd = commentEnd;
    }

    public string Name { get; }
    public string CommentStart { get; }
    public string CommentEnd { get; }
}