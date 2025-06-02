namespace Brimborium.Text;

/// <summary>
/// Represents the result of a search operation within a <see cref="StringSlice"/>.
/// Contains information about the found text and its position within the original string.
/// </summary>
public readonly struct StringSliceSearchResult {
    private readonly string _Text;
    private readonly int _OriginalOffset;
    private readonly int _Offset;
    private readonly int _Length;
    private readonly int _OriginalLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSliceSearchResult"/> struct.
    /// </summary>
    /// <param name="text">The original string that was searched.</param>
    /// <param name="orginalOffset">The starting offset of the slice within the original string.</param>
    /// <param name="offset">The offset where the search text was found.</param>
    /// <param name="length">The length of the found text.</param>
    /// <param name="originalLength">The length of the original slice that was searched.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the offsets or lengths are invalid.</exception>
    /// <remarks>
    /// Special case: When all offset and length parameters are -1, it represents a "not found" result.
    /// </remarks>
    public StringSliceSearchResult(
        string text,
        int orginalOffset,
        int offset,
        int length,
        int originalLength
        ) {
        if ((-1 == orginalOffset) && (-1 == offset) && (-1 == length) && (-1 == originalLength)) {
            // OK Fault values
        } else {
            if (text is null) {
                throw new ArgumentNullException(nameof(text));
            }
            var textLength = text.Length;

            if ((0 <= orginalOffset)
                && (0 <= originalLength)
                && ((orginalOffset + originalLength) <= textLength)) {
                // OK 
            } else {
                throw new ArgumentOutOfRangeException(nameof(orginalOffset));
            }
            if ((orginalOffset <= offset)
                && (0 <= length)
                && ((offset + length) <= (orginalOffset + originalLength))
                && ((offset + length) <= textLength)
                ) {
                // OK 
            } else {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }
        this._Text = text;
        this._OriginalOffset = orginalOffset;
        this._Offset = offset;
        this._Length = length;
        this._OriginalLength = originalLength;
    }

    /// <summary>
    /// Gets the original string that was searched.
    /// </summary> 
    public string Text => this._Text;

    /// <summary>
    /// Gets the starting offset of the slice within the original string.
    /// </summary> 
    public int OriginalOffset => this._OriginalOffset;

    /// <summary>
    /// Gets the offset where the search text was found.
    /// </summary> 
    public int Offset => this._Offset;

    /// <summary>
    /// Gets the length of the found text.
    /// </summary> 
    public int Length => this._Length;

    /// <summary>
    /// Gets the end position of the found text within the original string.
    /// </summary>
    public int End => this._Offset + this._Length;

    /// <summary>
    /// Gets the length of the original slice that was searched.
    /// </summary> 
    public int OriginalLength => this._OriginalLength;

    /// <summary>
    /// Gets the end position of the original slice within the original string.
    /// </summary>
    public int OriginalEnd => this._OriginalOffset + this._OriginalLength;


    /// <summary>
    /// Gets a <see cref="StringSlice"/> representing the text before the found text.
    /// </summary>
    public StringSlice Before => new StringSlice(this._Text, new Range(this._OriginalOffset, this._Offset));

    /// <summary>
    /// Gets a <see cref="StringSlice"/> representing the text before and including the found text.
    /// </summary>
    public StringSlice BeforeAndFound => new StringSlice(this._Text, new Range(this._OriginalOffset, this._Offset + this._Length));

    /// <summary>
    /// Gets a <see cref="StringSlice"/> representing only the found text.
    /// </summary>
    public StringSlice Found => new StringSlice(this._Text, new Range(this._Offset, this._Offset + this._Length));

    /// <summary>
    /// Gets a <see cref="StringSlice"/> representing the found text and all text after it.
    /// </summary>
    public StringSlice FoundAndAfter => new StringSlice(this._Text).Substring(this._Offset);

    /// <summary>
    /// Gets a <see cref="StringSlice"/> representing only the text after the found text.
    /// </summary>
    public StringSlice After => new StringSlice(this._Text).Substring(this._Offset + this._Length);
}
