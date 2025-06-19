namespace Brimborium.ReplaceContent;

/// <summary>
/// Represents the result of parsing content for placeholders.
/// </summary>
public class RCParseResult {
    /// <summary>
    /// Gets the list of parsed parts from the content.
    /// </summary>
    public readonly List<RCPart> ListPart;

    /// <summary>
    /// Initializes a new instance of the <see cref="RCParseResult"/> class.
    /// </summary>
    /// <param name="listRCPart">The list of parsed parts.</param>
    public RCParseResult(List<RCPart> listRCPart) {
        this.ListPart = listRCPart;
    }

    /// <summary>
    /// Gets a value indicating whether the parse result is valid.
    /// A valid result has no errors and follows the expected structure of placeholders.
    /// </summary>
    public bool IsValid {
        get {
            var currentListPart = this.ListPart;
            return IsValidRecursiv(currentListPart);

            static bool IsValidRecursiv(List<RCPart> currentListPart) {
                if ((currentListPart.Count > 0) && (currentListPart.All(p => p.ErrorMessage is null))) {
                    int index = 0;
                    while (index < currentListPart.Count) {
                        var part = currentListPart[index];
                        if (part.PartType == RCPartType.Error) {
                            return false;
                        }
                        if (part.PartType == RCPartType.ConstantText) {
                            index++;
                            continue;
                        } else if (part.PartType == RCPartType.PlaceholderStart) {
                            // Check if the next part is a PlaceholderEnd
                            if ((index + 2) < currentListPart.Count) {
                                var part1 = currentListPart[index + 1];
                                var part2 = currentListPart[index + 2];
                                if (part1.PartType == RCPartType.PlaceholderContent
                                    && part2.PartType == RCPartType.PlaceholderEnd
                                    && part1.PlaceholderName == part.PlaceholderName) {
                                    // Skip the PlaceholderContent and PlaceholderEnd
                                    index += 3;
                                    continue;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        } else if (part.PartType == RCPartType.PlaceholderEnd) {
                            // PlaceholderEnd without PlaceholderStart
                            return false;
                        } else if (part.PartType == RCPartType.PlaceholderContent) {
                            // PlaceholderContent without PlaceholderStart
                            return false;
                        } else {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Determines whether the parse result contains any errors.
    /// </summary>
    /// <param name="result">When this method returns, contains the first error part if found; otherwise, null.</param>
    /// <returns>true if the parse result contains errors; otherwise, false.</returns>
    public bool ContainsError([MaybeNullWhen(false)] out RCPart result) {
        if (ContainsErrorRecursive(this.ListPart, out result)){
            return true;
        }
        if (IsValid) {
            result = default;
            return false;
        } else {
            result = new RCPart(RCPartType.Error, string.Empty, "Is not valid", null, null);
            return true;
        }

        static bool ContainsErrorRecursive(List<RCPart> currentListPart, [MaybeNullWhen(false)] out RCPart failed) {
            foreach (var part in currentListPart) {
                if (part.ErrorMessage is not null) {
                    failed = part;
                    return true;
                }
            }
            failed = default;
            return false;
        }
    }

}
