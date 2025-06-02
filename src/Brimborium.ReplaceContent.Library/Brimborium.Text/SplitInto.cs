namespace Brimborium.Text;

/// <summary>
/// Represents the result of splitting a string into two parts: a found segment and the remaining tail.
/// </summary>
/// <remarks>This structure is typically used to represent the outcome of a string-splitting operation where a
/// specific segment is identified and separated from the rest of the string. Both <see cref="Found"/> and <see
/// cref="Tail"/> are immutable.</remarks>
/// <param name="Found">The segment of the string that was found during the split operation.</param>
/// <param name="Tail">The remaining part of the string after the found segment.</param>
public readonly record struct SplitInto(StringSlice Found, StringSlice Tail);
