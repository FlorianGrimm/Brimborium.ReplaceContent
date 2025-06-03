using Brimborium.Text;

namespace Brimborium.ReplaceContent;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class RCPart {

    public RCPart(
        RCPartType partType,
        string oldContent,
        string? errorMessage,
        string? placeholderName,
        string? indentation) {
        this.PartType = partType;
        this.OldContent = oldContent;
        this.ErrorMessage = errorMessage;
        this.PlaceholderName = placeholderName;
        this.Indentation = indentation;
    }

    public RCPartType PartType { get; }
    public string OldContent { get; }
    public string? ErrorMessage { get; }
    public string? PlaceholderName { get; }
    public string? Indentation { get; }
    public string? NextContent { get; set; }

    private string GetDebuggerDisplay() {
        StringSliceBuilder result = new();
        result.Append(this.PartType.ToString());
        result.Append(": ");
        if (OldContent.Length <= 32) {
            result.Append(this.OldContent);
        } else {
            result.Append(this.OldContent.AsStringSlice().Substring(0, 29)).Append("...");
        }
        result.Append(", ");
        if (this.PlaceholderName is null) {
            result.Append("NULL");
        } else if (this.PlaceholderName.Length <= 32) {
            result.Append(this.PlaceholderName);
        } else {
            result.Append(this.PlaceholderName.AsStringSlice().Substring(0, 29)).Append("...");
        }
        result.Append(", ");
        if (this.NextContent is null) {
            result.Append("NULL");
        } else if (this.NextContent.Length <= 32) {
            result.Append(this.NextContent);
        } else {
            result.Append(this.NextContent.AsStringSlice().Substring(0, 29)).Append("...");
        }
        return result.ToString();
    }
}

public enum RCPartType {
    ConstantText = 1,
    PlaceholderStart,
    PlaceholderEnd,
    PlaceholderContent,
    Error
}
