namespace Brimborium.ReplaceContent;

public sealed class RCContext
{
    public RCContext() {
    }
    
    public Dictionary<string, RCFileType> FileTypeByExtension { get; } = new (StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Placeholders { get; } = new ();
    public Dictionary<string, RCContent> Content { get; } = new ();
}
