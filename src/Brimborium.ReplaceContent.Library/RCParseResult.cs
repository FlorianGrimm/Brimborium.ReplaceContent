namespace Brimborium.ReplaceContent;

public class RCParseResult {
    public readonly List<RCPart> ListPart;

    public RCParseResult(List<RCPart> listRCPart) {
        this.ListPart = listRCPart;
    }

    public bool IsValid {
        get {
            if ((this.ListPart.Count > 0) && (this.ListPart.All(p => p.ErrorMessage is null))) {
                int index = 0;
                while (index < this.ListPart.Count) {
                    var part = this.ListPart[index];
                    if (part.PartType == RCPartType.Error) {
                        return false;
                    }
                    if (part.PartType == RCPartType.ConstantText) {
                        index++;
                        continue;
                    } else if (part.PartType == RCPartType.PlaceholderStart) {
                        // Check if the next part is a PlaceholderEnd
                        if ((index + 2) < this.ListPart.Count) {
                            var part1 = this.ListPart[index + 1];
                            var part2 = this.ListPart[index + 2];
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

    public bool ContainsError([MaybeNullWhen(false)] out RCPart result) {
        foreach (var part in this.ListPart) {
            if (part.ErrorMessage is not null) {
                result = part;
                return true;
            }
        }
        if (IsValid) {
            result = default;
            return false;
        } else {
            result = new RCPart(RCPartType.Error, string.Empty, "Is not valid", null, null);
            return true;
        }
    }

}
