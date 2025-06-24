namespace Brimborium.ReplaceContent;

/// <summary>
/// Represents a part of content being processed for placeholder replacement.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class RCPart {
    /// <summary>
    /// Initializes a new instance of the <see cref="RCPart"/> class.
    /// </summary>
    /// <param name="partType">The type of the part.</param>
    /// <param name="oldContent">The original content of the part.</param>
    /// <param name="errorMessage">An optional error message if there was an issue with this part.</param>
    /// <param name="placeholderName">The name of the placeholder if this part is related to a placeholder.</param>
    /// <param name="indentation">The indentation string to be applied to replacement content.</param>
    public RCPart(
        RCPartType partType,
        StringSlice oldContent,
        string? errorMessage,
        string? placeholderName,
        string? indentation) {
        this.PartType = partType;
        this.OldContent = oldContent;
        this.ErrorMessage = errorMessage;
        this.PlaceholderName = placeholderName;
        this.Indentation = indentation;
    }

    /// <summary>
    /// Gets the type of the part.
    /// </summary>
    public RCPartType PartType { get; }

    /// <summary>
    /// Gets the original content of the part.
    /// </summary>
    public StringSlice OldContent { get; }

    /// <summary>
    /// Gets an optional error message if there was an issue with this part.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the name of the placeholder if this part is related to a placeholder.
    /// </summary>
    public StringSlice PlaceholderName { get; }

    /// <summary>
    /// Gets the indentation string to be applied to replacement content.
    /// </summary>
    public StringSlice Indentation { get; }

    /// <summary>
    /// Gets or sets the content after placeholder replacement.
    /// </summary>
    public StringSlice? NextContent { get; set; }

    public List<RCPart> ListAST { get; } = new();


    /// <summary>
    /// Returns a string that represents the current object for debugging purposes.
    /// </summary>
    /// <returns>A string containing the part type, content preview, placeholder name, and next content.</returns>
    private string GetDebuggerDisplay() {
        StringSliceBuilder result = new();
        result.Append(this.PartType.ToString());
        result.Append(": ");
        if (OldContent.Length <= 32) {
            result.Append(this.OldContent);
        } else {
            result.Append(this.OldContent.Substring(0, 29)).Append("...");
        }
        result.Append(", ");
        if (this.PlaceholderName.IsEmpty) {
            result.Append("Empty");
        } else if (this.PlaceholderName.Length <= 32) {
            result.Append(this.PlaceholderName);
        } else {
            result.Append(this.PlaceholderName.Substring(0, 29)).Append("...");
        }
        result.Append(", ");
        if (this.NextContent is null) {
            result.Append("NULL");
        } else if (this.NextContent.Value.Length <= 32) {
            result.Append(this.NextContent.Value);
        } else {
            result.Append(this.NextContent.Value.Substring(0, 29)).Append("...");
        }
        return result.ToString();
    }
}
public static class RCPartExtension {
    public static bool TryHasPartType(this RCPart that, RCPartType rcPartType, [MaybeNullWhen(false)] out RCPart result) {
        if (that.PartType == rcPartType) {
            result = that;
            return true;
        } else {
            result = default;
            return false;
        }
    }

    public static bool IsEqualPlaceholderName(this RCPart thatStart, RCPart thatEnd, [MaybeNullWhen(false)] out StringSlice placeholderName) {
        if (thatStart.PlaceholderName is { IsEmpty: false } startPlaceholderName
            ) {
            if (thatEnd.PlaceholderName is { IsEmpty: false } endPlaceholderName) {
                if (startPlaceholderName.Equals(endPlaceholderName, StringComparison.Ordinal)) {
                    placeholderName = startPlaceholderName;
                    return true;
                }
            } else {
                placeholderName = startPlaceholderName;
                return true;
            }
        }
        {
            placeholderName = "".AsStringSlice();
            return false;
        }
    }

    public static bool TryHasPartType(this List<RCPart> that, int index, RCPartType rcPartType, [MaybeNullWhen(false)] out RCPart result) {
        if (index < that.Count) {
            result = that[index];
            return (result.PartType == rcPartType);
        } else {
            result = default;
            return false;
        }
    }
}
