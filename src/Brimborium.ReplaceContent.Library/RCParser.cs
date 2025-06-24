namespace Brimborium.ReplaceContent;

/// <summary>
/// Provides methods for parsing content with placeholders.
/// </summary>
public static class RCParser {
    private static readonly string _PlaceholderStartStart = "<Placeholder ";
    private static readonly string _PlaceholderEndStart = "</Placeholder";
    private static readonly string _DefaultStartStart = "<Default ";
    private static readonly string _DefaultEndStart = "</Default";
    private static readonly string _CustomizeStartStart = "<Customize ";
    private static readonly string _CustomizeEndStart = "</Customize";
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
    public static RCParseResult Parse(StringSlice? currentContent, string commentStart, string commentEnd) {
        List<RCPart> result = new();

        if (!(currentContent is { Length: > 0 } content)) { return new RCParseResult(result); }

        StringSlice commentStartAsSlice = commentStart.AsStringSlice();
        StringSlice commentEndAsSlice = commentEnd.AsStringSlice();

        var currentListAST = result;
        Stack<List<RCPart>> stackAST = new();
        stackAST.Push(currentListAST);

        string? currentPlaceholderName = null;
        Stack<string?> stackPlaceholderName = new ();
        stackPlaceholderName.Push(null);
        
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
                                currentListAST.Add(
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
                            stackPlaceholderName.Push(currentPlaceholderName = contentPlaceholderName);

                            //foundCommentEnd.BeforeAndFound
                            var currentAST=(new RCPart(
                                    partType: RCPartType.PlaceholderStart,
                                    oldContent: contentCompletePlaceholder.ToString(),
                                    errorMessage: null,
                                    placeholderName: contentPlaceholderName,
                                    indentation: currentIndentation));
                            currentListAST.Add(currentAST);
                            stackAST.Push(currentListAST = currentAST.ListAST);
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

                            stackPlaceholderName.Pop();
                            if (!stackPlaceholderName.TryPeek(out currentPlaceholderName)) {
                                errorMessage ??= $"inbalacned nested Placeholder";
                            }
                            stackAST.Pop();
                            if (!stackAST.TryPeek(out currentListAST)) {
                                errorMessage ??= "inbalanced nested Placeholder";
                                currentListAST ??= result;
                            }

                            if (0 < contentBeforeComment.Length) {
                                currentListAST.Add(
                                    new RCPart(
                                        partType: RCPartType.PlaceholderContent,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: errorMessage,
                                        placeholderName: contentPlaceholderName,
                                        indentation: null));
                            }

                            currentListAST.Add(
                                new RCPart(
                                    partType: RCPartType.PlaceholderEnd,
                                    oldContent: contentCompletePlaceholder.ToString(),
                                    errorMessage: errorMessage,
                                    placeholderName: contentPlaceholderName,
                                    indentation: null));

                            content = contentAfterLastFind = foundCommentEnd.After;
                        }
                        // --- Default ---
                        else if (contentPayload.StartsWith(_DefaultStartStart) && contentPayload.EndsWith(_PlaceholderStartEnd)) {
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(foundCommentStart.Found);
                            if (0 < contentBeforeComment.Length) {
                                currentListAST.Add(
                                    new RCPart(
                                        partType: RCPartType.ConstantText,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: null,
                                        placeholderName: null,
                                        indentation: null));
                                var currentIndentationSlice = contentBeforeComment.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(contentBeforeComment);
                                currentIndentation = 0 < currentIndentationSlice.Length ? currentIndentationSlice.ToString() : null;
                            }
                            var commentEndAfterWithoutTrailingWS = foundCommentEnd.After.TrimStart(_TabAndSpace).TrimStart(_CrLf);
                            var contentCompleteDefault = foundCommentStart.FoundAndAfter
                                .SubstringBetweenStartAndStart(commentEndAfterWithoutTrailingWS);

                            var contentDefaultName = contentPayload[new Range(new Index(_DefaultStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            stackPlaceholderName.Push(currentPlaceholderName = contentDefaultName);

                            var currentAST = new RCPart(
                                partType: RCPartType.DefaultStart,
                                oldContent: contentCompleteDefault.ToString(),
                                errorMessage: null,
                                placeholderName: contentDefaultName,
                                indentation: currentIndentation);
                            currentListAST.Add(currentAST);
                            stackAST.Push(currentListAST = currentAST.ListAST);
                            content = contentAfterLastFind = commentEndAfterWithoutTrailingWS;
                        }
                        else if (contentPayload.StartsWith(_DefaultEndStart) && contentPayload.EndsWith(_PlaceholderEndEnd)) {
                            var contentCompleteDefault = foundCommentStart.Before.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(foundCommentEnd.Found);
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(contentCompleteDefault);
                            var contentDefaultName = contentPayload[new Range(new Index(_DefaultStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            if (string.IsNullOrEmpty(contentDefaultName)) {
                                contentDefaultName = currentPlaceholderName;
                            }

                            string? errorMessage;
                            if (string.Equals(contentDefaultName, currentPlaceholderName)) {
                                errorMessage = null;
                            } else {
                                errorMessage = $"{contentDefaultName} expected, {currentPlaceholderName} found.";
                            }

                            stackPlaceholderName.Pop();
                            if (!stackPlaceholderName.TryPeek(out currentPlaceholderName)) {
                                errorMessage ??= $"inbalanced nested Default";
                            }
                            stackAST.Pop();
                            if (!stackAST.TryPeek(out currentListAST)) {
                                errorMessage ??= "inbalanced nested Default";
                                currentListAST ??= result;
                            }

                            if (0 < contentBeforeComment.Length) {
                                currentListAST.Add(
                                    new RCPart(
                                        partType: RCPartType.DefaultContent,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: errorMessage,
                                        placeholderName: contentDefaultName,
                                        indentation: null));
                            }

                            currentListAST.Add(
                                new RCPart(
                                    partType: RCPartType.DefaultEnd,
                                    oldContent: contentCompleteDefault.ToString(),
                                    errorMessage: errorMessage,
                                    placeholderName: contentDefaultName,
                                    indentation: null));

                            content = contentAfterLastFind = foundCommentEnd.After;
                        }
                        // --- Customize ---
                        else if (contentPayload.StartsWith(_CustomizeStartStart) && contentPayload.EndsWith(_PlaceholderStartEnd)) {
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(foundCommentStart.Found);
                            if (0 < contentBeforeComment.Length) {
                                currentListAST.Add(
                                    new RCPart(
                                        partType: RCPartType.ConstantText,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: null,
                                        placeholderName: null,
                                        indentation: null));
                                var currentIndentationSlice = contentBeforeComment.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(contentBeforeComment);
                                currentIndentation = 0 < currentIndentationSlice.Length ? currentIndentationSlice.ToString() : null;
                            }
                            var commentEndAfterWithoutTrailingWS = foundCommentEnd.After.TrimStart(_TabAndSpace).TrimStart(_CrLf);
                            var contentCompleteCustomize = foundCommentStart.FoundAndAfter
                                .SubstringBetweenStartAndStart(commentEndAfterWithoutTrailingWS);

                            var contentCustomizeName = contentPayload[new Range(new Index(_CustomizeStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            stackPlaceholderName.Push(currentPlaceholderName = contentCustomizeName);

                            var currentAST = new RCPart(
                                partType: RCPartType.CustomizeStart,
                                oldContent: contentCompleteCustomize.ToString(),
                                errorMessage: null,
                                placeholderName: contentCustomizeName,
                                indentation: currentIndentation);
                            currentListAST.Add(currentAST);
                            stackAST.Push(currentListAST = currentAST.ListAST);
                            content = contentAfterLastFind = commentEndAfterWithoutTrailingWS;
                        }
                        else if (contentPayload.StartsWith(_CustomizeEndStart) && contentPayload.EndsWith(_PlaceholderEndEnd)) {
                            var contentCompleteCustomize = foundCommentStart.Before.TrimEnd(_TabAndSpace).SubstringBetweenEndAndEnd(foundCommentEnd.Found);
                            var contentBeforeComment = contentAfterLastFind.SubstringBetweenStartAndStart(contentCompleteCustomize);
                            var contentCustomizeName = contentPayload[new Range(new Index(_CustomizeStartStart.Length, false), new Index(_PlaceholderStartEnd.Length, true))]
                                .Trim().ToString();
                            if (string.IsNullOrEmpty(contentCustomizeName)) {
                                contentCustomizeName = currentPlaceholderName;
                            }

                            string? errorMessage;
                            if (string.Equals(contentCustomizeName, currentPlaceholderName)) {
                                errorMessage = null;
                            } else {
                                errorMessage = $"{contentCustomizeName} expected, {currentPlaceholderName} found.";
                            }

                            stackPlaceholderName.Pop();
                            if (!stackPlaceholderName.TryPeek(out currentPlaceholderName)) {
                                errorMessage ??= $"inbalanced nested Customize";
                            }
                            stackAST.Pop();
                            if (!stackAST.TryPeek(out currentListAST)) {
                                errorMessage ??= "inbalanced nested Customize";
                                currentListAST ??= result;
                            }

                            if (0 < contentBeforeComment.Length) {
                                currentListAST.Add(
                                    new RCPart(
                                        partType: RCPartType.CustomizeContent,
                                        oldContent: contentBeforeComment.ToString(),
                                        errorMessage: errorMessage,
                                        placeholderName: contentCustomizeName,
                                        indentation: null));
                            }

                            currentListAST.Add(
                                new RCPart(
                                    partType: RCPartType.CustomizeEnd,
                                    oldContent: contentCompleteCustomize.ToString(),
                                    errorMessage: errorMessage,
                                    placeholderName: contentCustomizeName,
                                    indentation: null));

                            content = contentAfterLastFind = foundCommentEnd.After;
                        }
                        // --- Unknown ---
                        else {
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
                currentListAST.Add(
                    new RCPart(
                        partType: RCPartType.ConstantText,
                        oldContent: contentAfterLastFind.ToString(),
                        errorMessage: $"{currentPlaceholderName} is still open",
                        placeholderName: null,
                        indentation: null));
            } else {
                currentListAST.Add(
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

        return new RCParseResult(currentListAST);
    }
}
