namespace Brimborium.Text;

/// <summary>
/// Represents the state of a string slice, including the original text and the range of the slice.
/// </summary>
/// <remarks>This record struct is useful for scenarios where a portion of a string needs to be tracked or
/// manipulated while retaining information about the original text and the specific range being referenced.</remarks>
/// <param name="Text">The original string from which the slice is derived. Cannot be null.</param>
/// <param name="Range">The range within the original string that defines the slice.</param>
public record struct StringSliceState(string Text, Range Range);
