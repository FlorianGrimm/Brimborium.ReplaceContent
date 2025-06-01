namespace Brimborium.ReplaceContent;

public sealed class RCContent {
    public RCContent(string identifier) {
        this.Identifier = identifier;

    }

    public string Identifier { get;  }

    public string? FilePath { get; set; }
    public RCFileType? FileType { get; set; }
    public string? CurrentContent { get; set; }
    public string? NextContent { get; set; }


    public List<RCPart> ListPart { get; } = new();
}