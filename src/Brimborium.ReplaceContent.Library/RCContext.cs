namespace Brimborium.ReplaceContent;

public sealed class RCContext
{
    public RCContext() {
    }
    
    public Dictionary<string, RCFileType> FileTypeByExtension { get; } = new ();
    public Dictionary<string, string> Placeholders { get; } = new ();
    public Dictionary<string, RCContent> Content { get; } = new ();
}
