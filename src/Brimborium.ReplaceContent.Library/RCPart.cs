// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.ReplaceContent;

public class RCPart {

    public RCPart(RCPartType partType, string oldContent, string? placeholderName) {
        PartType = partType;
        OldContent = oldContent;
        PlaceholderName = placeholderName;
    }

    public RCPartType PartType { get; }
    public string OldContent { get; }
    public string? PlaceholderName { get; }
    public string? NextContent { get; set; }
}

public enum RCPartType {
    ConstantText = 1,
    PlaceholderStart,
    PlaceholderEnd,
    PlaceholderContent,
}