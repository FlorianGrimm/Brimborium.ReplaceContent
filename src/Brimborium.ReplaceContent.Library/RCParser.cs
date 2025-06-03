using System.Diagnostics.Metrics;

using Brimborium.Text;

namespace Brimborium.ReplaceContent;

/// <summary>
/// Provides methods for parsing content with placeholders.
/// </summary>
public static class RCParser {
    private static readonly string _PlaceholderStartStart = "<Placeholder ";
    private static readonly string _PlaceholderEndStart = "</Placeholder";
    private static readonly string _PlaceholderStartEnd = ">";
    private static readonly string _PlaceholderEndEnd = ">";
    private static readonly char[] _TabAndSpace = new char[] { '\t', ' ' };
    private static readonly char[] _CrLf = new char[] { '\r', '\n' };
    
    /// <summary>
    /// Parses the content and identifies placeholders within comment blocks.
    /// </summary>
    /// <param name="currentContent">The content to parse.</param>
    /// <param name="commentStart">The string that marks the start of a comment.</param>
    /// <param name="commentEnd">The string that marks the end of a comment.</param>
    /// <returns>A <see cref="RCParseResult"/> containing the parsed parts.</returns>
    public static RCParseResult Parse(string currentContent, string commentStart, string commentEnd) {
        List<RCPart> result = new();
        if (!(currentContent is { Length: > 0 })) { return new RCParseResult(result); }

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
                        if (contentPayload.StartsWith(_PlaceholderStartStart) && contentPayload.EndsWith(_PlaceholderStartEnd)) {
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


                            //var contentCompletePlaceholder = foundCommentStart.FoundAndAfter.SubstringBetweenStartAndEnd(foundCommentEnd.Found);
                            var commentEndAfterWithoutTrailingWS = foundCommentEnd.After.TrimStart(_TabAndSpace).TrimStart(_CrLf);
                            var contentCompletePlaceholder = foundCommentStart.FoundAndAfter
                                .SubstringBetweenStartAndStart(commentEndAfterWithoutTrailingWS);

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
                            content = contentAfterLastFind = commentEndAfterWithoutTrailingWS;

                        } else if (contentPayload.StartsWith(_PlaceholderEndStart) && contentPayload.EndsWith(_PlaceholderEndEnd)) {
                            // contentPayload=|</Placeholder xxx>|

                            var contentCompletePlaceholder = foundCommentStart.Before.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(foundCommentEnd.Found);

                            // contentBeforeComment=|>...<|/*
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(contentCompletePlaceholder);

                            // contentPlaceholderName = =</Placeholder |>xxx<|>
                            var contentPlaceholderName = contentPayload[new Range(new Index(_PlaceholderStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            if (string.IsNullOrEmpty(contentPlaceholderName)) {
                                contentPlaceholderName = currentPlaceholderName;
                            }

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
                            //content = contentAfterLastFind = commentEndAfterWithoutTrailingWS;
                        } else {
                            content = foundCommentEnd.After;
                        }
                    }
                } else {
                    content = foundCommentStart.After;
                }
            } else {
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

        return new RCParseResult(result);
    }
}
