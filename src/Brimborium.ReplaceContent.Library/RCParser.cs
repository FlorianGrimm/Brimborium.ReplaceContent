using Brimborium.Text;

namespace Brimborium.ReplaceContent;

public static class RCParser {
    private static readonly string _PlaceholderStartStart = "<Placeholder ";
    private static readonly string _PlaceholderEndStart = "</Placeholder";
    private static readonly string _PlaceholderStartEnd = ">";
    private static readonly string _PlaceholderEndEnd = ">";
    private static readonly char[] _TabAndSpace = new char[] { '\t', ' ' };

    public static ParseResult Parse(string currentContent, string commentStart, string commentEnd) {
        List<RCPart> result = new();
        if (!(currentContent is { Length: > 0 })) { return new ParseResult(result); }

        StringSlice content = currentContent.AsStringSlice();
        StringSlice commentStartAsSlice = commentStart.AsStringSlice();
        StringSlice commentEndAsSlice = commentEnd.AsStringSlice();
        string? currentPlaceholderName = null;
        string? currentIndentation = null;
        var contentAfterLastFind = content;
        while (0 < content.Length) {
            if (content.TryFind(commentStartAsSlice, out var foundCommentStart, StringComparison.Ordinal)) {
                // foundCommentStart=|/*| xxx */

                if (foundCommentStart.After.TryFind(commentEndAsSlice, out var foundCommentEnd, StringComparison.Ordinal)) {
                    {
                        if (foundCommentStart.After.TrySubstringBetweenStartAndStart(foundCommentEnd.Found, out var foundBetween)
                            && foundBetween.TryFind(commentStartAsSlice, out var foundCommentStartInner, StringComparison.Ordinal)) {
                            content = foundCommentStartInner.Found.SubstringBetweenStartAndEnd(content);
                            continue;
                        }
                    }
                    {
                        var contentPayload = foundCommentEnd.Before.Trim();
                        // check if the contentPayload is a placeholder start or end
                        if (contentPayload.StartsWith(_PlaceholderStartStart)
                            && contentPayload.EndsWith(_PlaceholderStartEnd)) {
                            // contentPayload=|<Placeholder xxx>|

                            // contentBeforeComment=|>...<|/*
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(foundCommentStart.Found);
                            if (0 < contentBeforeComment.Length) {
                                result.Add(
                                    new RCPart(
                                        partType: RCPartType.ConstantText,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: null,
                                        placeholderName: null,
                                        indentation: null));

                                var currentIndentationSlice = contentBeforeComment.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(contentBeforeComment);
                                if (0 < currentIndentationSlice.Length) {
                                    currentIndentation = currentIndentationSlice.ToString();
                                } else {
                                    currentIndentation = null;
                                }
                            }

                            var contentCompletePlaceholder = foundCommentStart.FoundAndAfter.SubstringBetweenStartAndEnd(foundCommentEnd.Found);

                            // contentPlaceholderName = =<Placeholder |>xxx<|>
                            var contentPlaceholderName = contentPayload[new Range(new Index(_PlaceholderStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            currentPlaceholderName = contentPlaceholderName;

                            //foundCommentEnd.BeforeAndFound
                            result.Add(new RCPart(
                                    partType: RCPartType.PlaceholderStart,
                                    oldContent: contentCompletePlaceholder.ToString(),
                                    errorMessage: null,
                                    placeholderName: contentPlaceholderName,
                                    indentation: currentIndentation));
                            content = contentAfterLastFind = foundCommentEnd.After;
                        } else if (contentPayload.StartsWith(_PlaceholderEndStart)
                            && contentPayload.EndsWith(_PlaceholderEndEnd)) {
                            // contentPayload=|</Placeholder xxx>|

                            // contentBeforeComment=|>...<|/*
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(foundCommentStart.Found);

                            // contentPlaceholderName = =</Placeholder |>xxx<|>
                            var contentPlaceholderName = contentPayload[new Range(new Index(_PlaceholderStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            if (string.IsNullOrEmpty(contentPlaceholderName)) {
                                contentPlaceholderName = currentPlaceholderName;
                            }

                            var contentCompletePlaceholder = foundCommentStart.FoundAndAfter.SubstringBetweenStartAndEnd(foundCommentEnd.Found);


                            string? errorMessage;
                            if (string.Equals(contentPlaceholderName, currentPlaceholderName)) {
                                errorMessage = null;
                            } else {
                                errorMessage = $"{contentPlaceholderName} expected, {currentPlaceholderName} found.";
                            }

                            if (0 < contentBeforeComment.Length) {
                                result.Add(
                                    new RCPart(
                                        partType: RCPartType.PlaceholderContent,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: errorMessage,
                                        placeholderName: contentPlaceholderName,
                                        indentation: null));
                            }

                            result.Add(
                                new RCPart(
                                    partType: RCPartType.PlaceholderEnd,
                                    oldContent: contentCompletePlaceholder.ToString(),
                                    errorMessage: errorMessage,
                                    placeholderName: contentPlaceholderName,
                                    indentation: null));

                            currentPlaceholderName = null;
                            content = contentAfterLastFind = foundCommentEnd.After;
                        } else {
                            content = foundCommentEnd.After;
                        }
                    }
                } else {
                    content = foundCommentStart.After;
                }
            } else {
                //var contentLength = contentAfterLastFind.Length;
                //if (0 < contentLength) {
                //    if (currentPlaceholderName is not null) {
                //        result.Add(
                //            new RCPart(
                //                partType: RCPartType.ConstantText,
                //                oldContent: content.ToString(),
                //                errorMessage: $"{currentPlaceholderName} is still open",
                //                placeholderName: null,
                //                indentation: currentIndentation));
                //    } else {
                //        result.Add(
                //            new RCPart(
                //                partType: RCPartType.ConstantText,
                //                oldContent: content.ToString(),
                //                errorMessage: null,
                //                placeholderName: null,
                //                indentation: null));
                //    }
                //    contentAfterLastFind = content = contentAfterLastFind.Substring(contentLength);
                //}
                break;
            }
        }

        // No comment start found, treat the rest as constant text
        if (contentAfterLastFind.Length > 0) {
            if (currentPlaceholderName is not null) {
                result.Add(
                    new RCPart(
                        partType: RCPartType.ConstantText,
                        oldContent: contentAfterLastFind.ToString(),
                        errorMessage: $"{currentPlaceholderName} is still open",
                        placeholderName: null,
                        indentation: null));
            } else {
                result.Add(
                    new RCPart(
                        partType: RCPartType.ConstantText,
                        oldContent: contentAfterLastFind.ToString(),
                        errorMessage: null,
                        placeholderName: null,
                        indentation: null));
            }
            content = contentAfterLastFind.Substring(contentAfterLastFind.Length);
        }

        if (0 < content.Length) {
            throw new ArgumentException("Content could not be parsed completely. Remaining content: " + content.ToString(), nameof(currentContent));
        }

        return new ParseResult(result);
    }
}
public class ParseResult {
    public readonly List<RCPart> ListPart;

    public ParseResult(List<RCPart> listRCPart) {
        this.ListPart = listRCPart;
    }

    public bool IsValid => (this.ListPart.Count > 0) && (this.ListPart.All(p => p.ErrorMessage is null));

    public bool ContainsError([MaybeNullWhen(false)] out RCPart result) {
        foreach (var part in this.ListPart) {
            if (part.ErrorMessage is not null) {
                result = part;
                return true;
            }
        }
        {
            result = default;
            return false;
        }
    }

}
