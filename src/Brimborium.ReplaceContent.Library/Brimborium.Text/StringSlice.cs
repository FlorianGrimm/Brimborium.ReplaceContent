namespace Brimborium.Text;

/// <summary>
/// Represents a lightweight, immutable view into a portion of a string.
/// </summary>
/// <remarks>
/// <para>
/// Value provides a memory-efficient way to work with substrings without copying the underlying string data.
/// Unlike <see cref="ReadOnlySpan{T}"/>, Value can be used in async methods and stored in fields.
/// </para>
/// <para>
/// The struct maintains a reference to the original string and a Range that defines the viewed portion.
/// All operations create new Value instances or return values based on the defined range.
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var slice = new Value("Hello World");
/// var hello = slice.Left(0, 5);    // Creates slice for "Hello"
/// var world = slice.Left(6, 5);    // Creates slice for "World"
/// Console.WriteLine(hello.ToString());   // Outputs: "Hello"
/// </code>
/// </example>
[System.Diagnostics.DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct StringSlice : IEquatable<StringSlice> {
    /// <summary>
    /// Gets an empty Value instance.
    /// </summary>
    public static StringSlice Empty => new StringSlice(string.Empty);

    private readonly string _Text = String.Empty;

    private readonly Range _Range = Range.All;

    /// <summary>
    /// Gets the underlying string that this slice references.
    /// </summary>
    /// <remarks>
    /// This property returns the complete original string, not just the sliced portion.
    /// Use <see cref="ToString"/> to get the actual slice content.
    /// </remarks>
    [System.Text.Json.Serialization.JsonInclude()]
    [System.Text.Json.Serialization.JsonPropertyOrder(0)]
    public string Text => this._Text;

    /// <summary>
    /// Gets the range that defines the boundaries of this slice within the original string.
    /// </summary>
    /// <remarks>
    /// The range values are always from the start of the string (not from end).
    /// </remarks>
    public readonly Range Range => this._Range;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSlice"/> struct with an empty string.
    /// </summary>
    public StringSlice() {
        this._Text = string.Empty;
        this._Range = new Range(0, 0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSlice"/> struct with the specified text.
    /// </summary>
    /// <param name="text">The string to create a slice from.</param>
    /// <remarks>
    /// Creates a slice that spans the entire input string.
    /// </remarks>
    public StringSlice(
       string text
     ) {
        this._Text = text ?? string.Empty;
        this._Range = new Range(0, this.Text.Length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSlice"/> struct with the specified text and range.
    /// </summary>
    /// <param name="text">The string to create a slice from.</param>
    /// <param name="range">The range within the string to slice.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>The range start is greater than the string length</description></item>
    /// <item><description>The range end is greater than the string length</description></item>
    /// <item><description>The range end is less than the range start</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// If the range uses from-end indices (^), they are converted to from-start indices.
    /// </remarks>
    public StringSlice(
        string text,
        Range range) {
        if (range.Start.IsFromEnd || range.End.IsFromEnd) {
            var (offset, length) = range.GetOffsetAndLength(text.Length);
            range = new Range(offset, offset + length);
        }
        if (text.Length < range.Start.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (text.Length < range.End.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (range.End.Value < range.Start.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }

        this._Text = text;
        this._Range = range;
    }

    /// <summary>
    /// Gets the character at the specified index within the slice.
    /// </summary>
    /// <param name="index">The zero-based index of the character to get.</param>
    /// <returns>The character at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is negative or greater than or equal to the length of the slice.
    /// </exception>
    /// <remarks>
    /// The index is relative to the start of the slice, not the start of the underlying string.
    /// </remarks>
    public char this[int index] {
        get {
            var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
            var end = offset + length;
            if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index)); }
            if (length <= index) { throw new ArgumentOutOfRangeException(nameof(index)); }
            return this.Text[offset + index];
        }
    }

    /// <summary>
    /// Get the slice of the string defined by the specified range.
    /// </summary>
    /// <param name="range">The range is commulative to the existing range.</param>
    /// <returns>The slice</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is negative or greater than or equal to the length of the slice.
    /// </exception>
    public StringSlice this[Range range] {
        get {
            var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
            var end = offset + length;
            var (rangeOffset, rangeLength) = range.GetOffsetAndLength(length);
            if (rangeOffset < 0) { throw new ArgumentOutOfRangeException(nameof(range)); }
            if (length < rangeLength) { throw new ArgumentOutOfRangeException(nameof(range)); }
            return new StringSlice(
                this.Text,
                new Range(offset + rangeOffset, offset + rangeOffset + rangeLength)
                );
        }
    }

    /// <summary>
    /// Calculates the start thisOffset and length of the range object using a collection length.
    /// </summary>
    /// <returns>The start thisOffset and length of the range.</returns>
    public (int Offset, int Length) GetOffsetAndLength()
        => (this.Text is null)
            ? (Offset: 0, Length: 0)
            : this.Range.GetOffsetAndLength(this.Text.Length);

    /// <summary>
    /// The offset(start) of the range.
    /// </summary>
    public int GetOffset() {
        var (offset, _) = this.GetOffsetAndLength();
        return offset;
    }

    /// <summary>
    /// Calculates the ending position based on the offset and length.
    /// </summary>
    /// <returns>The sum of the offset and length, representing the ending position.</returns>
    public int GetEnd() {
        var (offset, length) = this.GetOffsetAndLength();
        return offset + length;
    }

    /// <summary>
    /// Creates a new Value that is a substring of this slice, starting at the specified index.
    /// </summary>
    /// <param name="start">The zero-based starting character position of the substring in this slice.</param>
    /// <returns>A new Value that begins at the specified position in this slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than zero or greater than the length of this slice.
    /// </exception>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted range.
    /// </remarks>
    public StringSlice Substring(int start) {
        if (start < 0) { throw new ArgumentOutOfRangeException(nameof(start)); }
        var thisOffset = this.Range.Start.Value;
        var thisEnd = this.Range.End.Value;
        var thisLength = thisEnd - thisOffset;
        if (thisLength < start) { throw new ArgumentOutOfRangeException(nameof(start)); }
        var nextRange = new Range(thisOffset + start, thisOffset + thisLength);
        return new StringSlice(
            this.Text,
            nextRange
            );
    }

    /// <summary>
    /// Creates a new Value that is a substring of this slice, starting at the specified index and having the specified length.
    /// </summary>
    /// <param name="start">The zero-based starting character position of the substring in this slice.</param>
    /// <param name="length">The number of characters in the substring.</param>
    /// <returns>A new Value that begins at the specified position in this slice and has the specified length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para><paramref name="start"/> is less than zero.</para>
    /// <para>-or-</para>
    /// <para><paramref name="length"/> is less than zero.</para>
    /// <para>-or-</para>
    /// <para><paramref name="length"/> is greater than the length of this slice minus <paramref name="start"/>.</para>
    /// </exception>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted range.
    /// </remarks>
    public StringSlice Substring(int start, int length) {
        if (start < 0) { throw new ArgumentOutOfRangeException(nameof(start)); }
        if (length < 0) { throw new ArgumentOutOfRangeException(nameof(length)); }

        var thisOffset = this.Range.Start.Value;
        var thisEnd = this.Range.End.Value;
        var thisLength = thisEnd - thisOffset;

        if (thisLength < length) { throw new ArgumentOutOfRangeException(nameof(length)); }
        var nextRange = new Range(thisOffset + start, thisOffset + start + length);
        return new StringSlice(
            this.Text,
            nextRange
            );
    }

    /// <summary>
    /// Creates a new Value that is a substring of this slice, based on the specified range.
    /// </summary>
    /// <param name="range">A range that describes the portion of this slice to include in the new slice.</param>
    /// <returns>A new Value that corresponds to the specified range of this slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>The calculated start position is greater than the calculated end position.</para>
    /// <para>-or-</para>
    /// <para>The calculated range extends beyond the bounds of this slice.</para>
    /// </exception>
    /// <remarks>
    /// The range can use from-end indices (^) which will be calculated relative to the length of this slice.
    /// </remarks>
    public StringSlice Substring(Range range) {
        // shortcut because range is from start
        var thisOffset = this.Range.Start.Value;
        var thisEnd = this.Range.End.Value;
        var thisLength = thisEnd - thisOffset;

        // range can be from end so I use GetOffsetAndLength
        var (rangeOffset, rangeLength) = range.GetOffsetAndLength(thisLength);

        var nextRange = new Range(thisOffset + rangeOffset, thisOffset + rangeOffset + rangeLength);
        if (nextRange.Start.Value > nextRange.End.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (thisEnd < nextRange.Start.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (thisEnd < nextRange.End.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }

        return new StringSlice(
            this.Text,
            nextRange
            );
    }

    /// <summary>
    /// Creates a new Value that is a substring of this slice, based on this range's offset and the specified range's offset.
    /// </summary>
    /// <param name="length">A range's offset that describes the portion of this slice to include in the new slice.</param>
    /// <returns>A new Value that corresponds to the specified range of this slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// the two StringSlice are not from the same string.
    /// </exception>>
    public StringSlice Left(StringSlice length) {
        if (!ReferenceEquals(this.Text, length.Text)) {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
        var (thisOffset, thisLength) = this.GetOffsetAndLength();
        var (lengthOffset, _) = length.GetOffsetAndLength();
        if (thisOffset > lengthOffset) {
            throw new ArgumentOutOfRangeException("length.Offset");
        }
        return new StringSlice(this.Text, thisOffset..lengthOffset);
    }

    /// <summary>
    /// Extracts a substring from the current <see cref="StringSlice"/> that starts at the same position and ends at the
    /// start of the specified <paramref name="other"/> slice.
    /// </summary>
    /// <param name="other">A <see cref="StringSlice"/> that defines the end boundary of the substring. The <paramref name="other"/> slice
    /// must reference the same underlying text as the current slice and must fall within the bounds of the current
    /// slice.</param>
    /// <returns>A new <see cref="StringSlice"/> representing the substring that starts at the beginning of the current slice and
    /// ends at the start of the <paramref name="other"/> slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> references a different text than the current slice, or if its offset is
    /// outside the bounds of the current slice.</exception>
    public StringSlice SubstringBetweenStartAndStart(StringSlice other) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var (thisOffset, thisLength) = this.GetOffsetAndLength();
        var (otherOffset, _) = other.GetOffsetAndLength();
        if (thisOffset > otherOffset) {
            throw new ArgumentOutOfRangeException("other.Offset");
        }
        if (otherOffset > this._Text.Length) {
            throw new ArgumentOutOfRangeException("other.Offset");
        }
        return new StringSlice(this.Text, thisOffset..otherOffset);
    }

    /// <summary>
    /// Extracts a substring from the current <see cref="StringSlice"/> that starts at its beginning and ends at the end
    /// of the specified <paramref name="other"/> slice.
    /// </summary>
    /// <param name="other">A <see cref="StringSlice"/> that defines the end boundary for the substring.  The <see cref="Text"/> property of
    /// <paramref name="other"/> must reference the same string as the current instance.</param>
    /// <returns>A new <see cref="StringSlice"/> representing the substring that starts at the beginning of the current slice and
    /// ends at the end of <paramref name="other"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> references a different string than the current instance,  or if the end of
    /// <paramref name="other"/> extends beyond the bounds of the current slice.</exception>
    public StringSlice SubstringBetweenStartAndEnd(StringSlice other) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var (thisOffset, thisLength) = this.GetOffsetAndLength();
        var otherEnd = other.GetEnd();
        if (thisOffset > otherEnd) {
            throw new ArgumentOutOfRangeException("other");
        }
        return new StringSlice(this.Text, thisOffset..otherEnd);
    }

    /// <summary>
    /// Extracts a substring from the current <see cref="StringSlice"/> that starts at the end of this slice and ends at
    /// the start of the specified <paramref name="other"/> slice.
    /// </summary>
    /// <param name="other">The <see cref="StringSlice"/> that defines the endpoint of the substring. Must reference the same underlying
    /// text as this slice.</param>
    /// <returns>A new <see cref="StringSlice"/> representing the substring between the end of this slice and the start of
    /// <paramref name="other"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> references a different text than this slice, or if the end of this slice is
    /// positioned after the start of <paramref name="other"/>.</exception>
    public StringSlice SubstringBetweenEndAndStart(StringSlice other) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var thisEnd = this.GetEnd();
        var otherOffset = other.GetOffset();
        if (thisEnd > otherOffset) {
            throw new ArgumentOutOfRangeException("other.Offset");
        }
        return new StringSlice(this.Text, thisEnd..otherOffset);
    }

    /// <summary>
    /// Extracts a substring from the current <see cref="StringSlice"/> that starts at the end of this slice and ends at
    /// the end of the specified <paramref name="other"/> slice.
    /// </summary>
    /// <param name="other">A <see cref="StringSlice"/> that shares the same underlying text as the current slice. The method uses the end
    /// position of this slice and the end position of <paramref name="other"/> to determine the substring range.</param>
    /// <returns>A new <see cref="StringSlice"/> representing the substring between the end of the current slice and the end of
    /// the specified <paramref name="other"/> slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> does not share the same underlying text as the current slice, or if the end
    /// position of the current slice is greater than the end position of <paramref name="other"/>.</exception>
    public StringSlice SubstringBetweenEndAndEnd(StringSlice other) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var thisEnd = this.GetEnd();
        var otherEnd = other.GetEnd();
        if (thisEnd > otherEnd) {
            throw new ArgumentOutOfRangeException("other.End");
        }
        return new StringSlice(this.Text, thisEnd..otherEnd);
    }

    /// <summary>
    /// Attempts to extract a substring from the current <see cref="StringSlice"/> that starts at the same position as
    /// this instance and ends at the start position of the specified <paramref name="other"/>.
    /// </summary>
    /// <remarks>This method ensures that the substring is valid by checking the relative positions of the
    /// current instance and <paramref name="other"/>. If the start position of the current instance is greater than the
    /// start position of <paramref name="other"/>, or if the end of the current instance exceeds the start position of
    /// <paramref name="other"/>, the method returns <see langword="false"/> and sets <paramref name="result"/> to an
    /// empty <see cref="StringSlice"/>.</remarks>
    /// <param name="other">A <see cref="StringSlice"/> that defines the end boundary for the substring. The <see cref="Text"/> property of
    /// <paramref name="other"/> must reference the same string as the current instance.</param>
    /// <param name="result">When this method returns, contains the resulting substring as a <see cref="StringSlice"/> if the operation
    /// succeeds; otherwise, contains an empty <see cref="StringSlice"/>.</param>
    /// <returns><see langword="true"/> if a valid substring is extracted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="Text"/> property of <paramref name="other"/> does not reference the same string as the
    /// current instance.</exception>
    public bool TrySubstringBetweenStartAndStart(StringSlice other, out StringSlice result) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var (thisOffset, thisLength) = this.GetOffsetAndLength();
        var (otherOffset, _) = other.GetOffsetAndLength();
        if (thisOffset > otherOffset) {
            result = this.Substring(0, 0);
            return false;
        }
        if (otherOffset > this._Text.Length) {
            result = this.Substring(0, 0);
            return false;
        }
        result = new StringSlice(this.Text, thisOffset..otherOffset);
        return true;
    }

    /// <summary>
    /// Attempts to extract a substring from the current <see cref="StringSlice"/> that starts at its beginning and ends
    /// at the end of the specified <paramref name="other"/> <see cref="StringSlice"/>.
    /// </summary>
    /// <remarks>This method checks whether the current <see cref="StringSlice"/> overlaps with or is adjacent
    /// to the specified <paramref name="other"/>. If the current slice starts after the end of <paramref
    /// name="other"/>, the operation fails and <paramref name="result"/> is set to an empty <see
    /// cref="StringSlice"/>.</remarks>
    /// <param name="other">The <see cref="StringSlice"/> that defines the end boundary for the substring. Must reference the same
    /// underlying text as the current <see cref="StringSlice"/>.</param>
    /// <param name="result">When this method returns, contains the resulting substring as a <see cref="StringSlice"/> if the operation
    /// succeeds, or an empty <see cref="StringSlice"/> if it fails.</param>
    /// <returns><see langword="true"/> if a valid substring was extracted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> does not reference the same underlying text as the current <see
    /// cref="StringSlice"/>.</exception>
    public bool TrySubstringBetweenStartAndEnd(StringSlice other, out StringSlice result) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var (thisOffset, thisLength) = this.GetOffsetAndLength();
        var otherEnd = other.GetEnd();
        if (thisOffset > otherEnd) {
            result = this.Substring(0, 0);
            return false;
        }
        result = new StringSlice(this.Text, thisOffset..otherEnd);
        return true;
    }

    /// <summary>
    /// Attempts to extract a substring from the current <see cref="StringSlice"/> that starts at the end of this
    /// instance and ends at the start of the specified <paramref name="other"/> <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="other">The <see cref="StringSlice"/> that defines the end boundary for the substring. Must reference the same
    /// underlying text as this instance.</param>
    /// <param name="result">When this method returns, contains the resulting substring as a <see cref="StringSlice"/> if the operation
    /// succeeds; otherwise, contains an empty <see cref="StringSlice"/>.</param>
    /// <returns><see langword="true"/> if a valid substring is extracted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> does not reference the same underlying text as this instance.</exception>
    public bool TrySubstringBetweenEndAndStart(StringSlice other, out StringSlice result) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var thisEnd = this.GetEnd();
        var otherOffset = other.GetOffset();
        if (thisEnd > otherOffset) {
            result = this.Substring(0, 0);
            return false;
        }
        result = new StringSlice(this.Text, thisEnd..otherOffset);
        return true;
    }

    /// <summary>
    /// Attempts to extract a substring from the current <see cref="StringSlice"/> that starts at the end of this slice
    /// and ends at the end of the specified <paramref name="other"/> slice.
    /// </summary>
    /// <remarks>This method compares the end positions of the current slice and <paramref name="other"/>. If
    /// the end of this slice is after the end of <paramref name="other"/>, the operation fails, and <paramref
    /// name="result"/> is set to an empty slice.</remarks>
    /// <param name="other">The <see cref="StringSlice"/> that defines the end boundary for the substring. Must reference the same
    /// underlying text as this slice.</param>
    /// <param name="result">When this method returns, contains the resulting substring as a <see cref="StringSlice"/> if the operation
    /// succeeds; otherwise, contains an empty <see cref="StringSlice"/>.</param>
    /// <returns><see langword="true"/> if the substring was successfully extracted; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="other"/> does not reference the same underlying text as this slice.</exception>
    public bool TrySubstringBetweenEndAndEnd(StringSlice other, out StringSlice result) {
        if (!ReferenceEquals(this.Text, other.Text)) {
            throw new ArgumentOutOfRangeException(nameof(other));
        }
        var thisEnd = this.GetEnd();
        var otherEnd = other.GetEnd();
        if (thisEnd > otherEnd) {
            result = this.Substring(0, 0);
            return false;
        }
        result = new StringSlice(this.Text, thisEnd..otherEnd);
        return true;
    }

    /// <summary>
    /// Gets the text and range that define this slice.
    /// </summary>
    /// <returns>A <see cref="StringSliceState"/> containing the underlying text and range of this slice.</returns>
    public StringSliceState GetTextAndRange()
        => new StringSliceState(this.Text, this.Range);

    /// <summary>
    /// Deconstructs the Value into its component parts.
    /// </summary>
    /// <param name="text">When this method returns, contains the underlying text of the slice.</param>
    /// <param name="range">When this method returns, contains the range that defines the boundaries of the slice.</param>
    public void Deconstruct(out string text, out Range range) {
        text = this.Text;
        range = this.Range;
    }

    /// <summary>
    /// Gets the length of characters in the slice.
    /// </summary>
    /// <remarks>
    /// Returns the number of characters in the sliced portion, not the length of the underlying string.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore()]
    public int Length {
        get {
            // shortcut because this.Range is from start
            var (_, length) = this.Range.GetOffsetAndLength(this.Text.Length);
            return length;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the slice is empty (contains no characters).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore()]
    public bool IsEmpty {
        get {
            // shortcut because this.Range is from start
            var (_, length) = this.Range.GetOffsetAndLength(this.Text.Length);
            return 0 == length;
        }
    }

    /// <summary>
    /// Returns a new string that represents the current slice.
    /// </summary>
    /// <returns>A new string containing the characters from this slice.</returns>
    /// <remarks>
    /// This method creates a new string instance if the slice doesn't span the entire original string.
    /// </remarks>
    public override string ToString() {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;

        if (length == 0) { return string.Empty; }
        if (offset == 0 && length == this.Text.Length) {
            return this.Text;
        } else {
            return this.Text.Substring(offset, length);
        }
    }

    /// <summary>
    /// Creates a <see cref="ReadOnlySpan{T}"/> from this slice.
    /// </summary>
    /// <returns>A readonly span representing the same range of characters as this slice.</returns>
    public ReadOnlySpan<char> AsSpan() {
        if (this.Text is null) { return ReadOnlySpan<char>.Empty; }
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        return this.Text.AsSpan(offset, length);
    }

    /// <summary>
    /// Creates a <see cref="ReadOnlySpan{T}"/> from a specified range within this slice.
    /// </summary>
    /// <param name="range">The range within this slice to convert to a span. The range is relative to the current slice's boundaries.</param>
    /// <returns>A readonly span representing the specified range of characters within this slice.</returns>
    /// <remarks>
    /// This method allows for creating a span from a subset of the current slice. The range parameter is evaluated
    /// relative to the current slice's boundaries, not the original string's boundaries.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified range extends beyond the boundaries of the current slice.
    /// </exception>
    /// <example>
    /// <code>
    /// var slice = new Value("Hello World");
    /// var span = slice.AsSpan(1..4); // Returns span containing "ell"
    /// </code>
    /// </example>
    public ReadOnlySpan<char> AsSpan(Range range) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        return this.Text.AsSpan(offset, length)[range];
    }

    /// <summary>
    /// Creates a <see cref="ReadOnlyMemory{T}"/> from this slice.
    /// </summary>
    /// <returns>A readonly memory representing the same range of characters as this slice.</returns>
    public ReadOnlyMemory<char> AsMemory()
        => this.Text.AsMemory(this.Range);

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private string GetDebuggerDisplay() {
        if (this.Text is null) { return "null"; }
        StringSliceBuilder builder = new();
        var (offset, length) = this.GetOffsetAndLength();
        builder.Append(offset.ToString());
        builder.Append("..");
        builder.Append((offset + length).ToString());
        builder.Append(":");
        if (this.Length < 32) {
            builder.Append(this.Text[this.Range]);
        } else {
            builder.Append(this.Text[new Range(this.Range.Start, this.Range.Start.Value + 29)]);
            builder.Append("...");
        }
        return builder.ToString();
    }

    /// <summary>
    /// Determines whether this slice represents a null or empty string.
    /// </summary>
    /// <returns>true if this slice's text is null or its length is 0; otherwise, false.</returns>
    public bool IsNullOrEmpty() {
        return this.Text is null || this.Length == 0;
    }

    /// <summary>
    /// Determines whether this slice consists of only white-space characters.
    /// </summary>
    /// <returns>true if this slice's text is null, empty, or consists entirely of white-space characters; otherwise, false.</returns>
    public bool IsNullOrWhiteSpace() {
        if (this.Text == null) {
            return true;
        }
        return this.AsSpan().IsWhiteSpace();
    }

    /// <summary>
    /// Attempts to get the first character in the slice.
    /// </summary>
    /// <param name="ch">When this method returns, contains the first character in the slice if the slice is not empty; otherwise, the default value for <see cref="char"/>.</param>
    /// <returns>true if the slice is not empty and the first character was successfully retrieved; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful when you need to check if a slice has any characters and get the first one in a single operation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slice = new StringSlice("Hello");
    /// if (slice.TryGetFirst(out var firstChar))
    /// {
    ///     Console.WriteLine(firstChar); // Outputs: 'H'
    /// }
    /// 
    /// var emptySlice = new StringSlice("");
    /// if (!emptySlice.TryGetFirst(out var _))
    /// {
    ///     Console.WriteLine("Slice is empty");
    /// }
    /// </code>
    /// </example>
    public bool TryGetFirst(out char ch) {
        var (offset, length) = this.Range.GetOffsetAndLength(this._Text.Length);
        if (0 == length) {
            ch = default;
            return false;
        } else {
            ch = this._Text[offset];
            return true;
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character within this slice.
    /// </summary>
    /// <param name="search">The character to seek.</param>
    /// <returns>The zero-based index position of the <paramref name="search"/> character if found, or -1 if not found.</returns>
    /// <remarks>
    /// The index is relative to the start of the slice, not the start of the underlying string.
    /// </remarks>
    public int IndexOf(char search) {
        return this.AsSpan().IndexOf(search);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character within the specified range of this slice.
    /// </summary>
    /// <param name="search">The character to seek.</param>
    /// <param name="range">The range within this slice to search.</param>
    /// <returns>The zero-based index position of the <paramref name="search"/> character if found, or -1 if not found.</returns>
    /// <remarks>
    /// The returned index is relative to the start of the specified range within the slice.
    /// </remarks>
    public int IndexOf(char search, Range range) {
        var thisOffset = this.Range.Start.Value;
        var thisEnd = this.Range.End.Value;
        var thisLength = thisEnd - thisOffset;

        var (offset, length) = range.GetOffsetAndLength(thisLength);
        var result = this.Text.AsSpan(offset + thisOffset, length).IndexOf(search);
        if (result < 0) {
            return -1;
        } else {
            return result + offset;
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified array within this slice.
    /// </summary>
    /// <param name="search">A character array containing one or more characters to seek.</param>
    /// <returns>The zero-based index position of the first occurrence of any character in <paramref name="search"/> if found, or -1 if not found.</returns>
    /// <remarks>
    /// The index is relative to the start of the slice, not the start of the underlying string.
    /// </remarks>
    public int IndexOfAny(char[] search) {
        return this.AsSpan().IndexOfAny(search);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified array within the specified range of this slice.
    /// </summary>
    /// <param name="search">A character array containing one or more characters to seek.</param>
    /// <param name="range">The range within this slice to search.</param>
    /// <returns>The zero-based index position of the first occurrence of any character in <paramref name="search"/> if found, or -1 if not found.</returns>
    /// <remarks>
    /// The returned index is relative to the start of the specified range within the slice.
    /// </remarks>
    public int IndexOfAny(char[] search, Range range) {
        var thisOffset = this.Range.Start.Value;
        var thisEnd = this.Range.End.Value;
        var thisLength = thisEnd - thisOffset;

        var (offset, length) = range.GetOffsetAndLength(thisLength);
        var result = this.Text.AsSpan(offset + thisOffset, length).IndexOfAny(search);
        if (result < 0) {
            return -1;
        } else {
            return result + offset;
        }
    }
    /// <summary>
    /// Attempts to find the first occurrence of a specified character sequence within this slice.
    /// </summary>
    /// <param name="searchFor">The character sequence to search for.</param>
    /// <param name="result">When this method returns, contains a <see cref="StringSliceSearchResult"/> with information about the search result if found; otherwise, contains default values.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>true if the character sequence is found; otherwise, false.</returns>
    /// <remarks>
    /// The search result provides access to the text before the match, the matched text, and the text after the match.
    /// The indices in the result are relative to the original string, not the slice.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slice = new StringSlice("Hello World");
    /// if (slice.TryFind(" ".AsStringSlice(), out var result)) {
    ///     Console.WriteLine(result.Before.ToString()); // Outputs: "Hello"
    ///     Console.WriteLine(result.After.ToString()); // Outputs: "World"
    /// }
    /// </code>
    /// </example>
    public bool TryFind(StringSlice searchFor, out StringSliceSearchResult result, StringComparison comparison = StringComparison.Ordinal) {
        return this.TryFind(searchFor.AsSpan(), out result, comparison);
    }

    /// <summary>
    /// Attempts to find the first occurrence of a specified character sequence within this slice.
    /// </summary>
    /// <param name="searchFor">The character sequence to search for.</param>
    /// <param name="result">When this method returns, contains a <see cref="StringSliceSearchResult"/> with information about the search result if found; otherwise, contains default values.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>true if the character sequence is found; otherwise, false.</returns>
    /// <remarks>
    /// The search result provides access to the text before the match, the matched text, and the text after the match.
    /// The indices in the result are relative to the original string, not the slice.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slice = new StringSlice("Hello World");
    /// if (slice.TryFind(" ".AsSpan(), out var result)) {
    ///     Console.WriteLine(result.Before.ToString()); // Outputs: "Hello"
    ///     Console.WriteLine(result.After.ToString()); // Outputs: "World"
    /// }
    /// </code>
    /// </example>
    public bool TryFind(ReadOnlySpan<char> searchFor, out StringSliceSearchResult result, StringComparison comparison = StringComparison.Ordinal) {
        var position = this.AsSpan().IndexOf(searchFor, comparison);
        if (0 <= position) {
            var (thisOffset, thisLength) = this.GetOffsetAndLength();
            var start = thisOffset + position;
            result = new StringSliceSearchResult(this._Text, thisOffset, start, searchFor.Length, thisLength);
            return true;
        } else {
            result = new StringSliceSearchResult(this._Text, -1, -1, -1, -1);
            return false;
        }
    }

    /// <summary>
    /// Determines whether this slice contains the specified character.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <returns>true if the character is found within this slice; otherwise, false.</returns>
    public bool Contains(char value) {
        return this.AsSpan().Contains(value);
    }

    /// <summary>
    /// Determines whether this slice contains the specified character sequence.
    /// </summary>
    /// <param name="value">The character sequence to seek.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>true if the sequence is found within this slice; otherwise, false.</returns>
    public bool Contains(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal) {
        return this.AsSpan().Contains(value, comparisonType);
    }

    /// <summary>
    /// Determines whether this slice starts with the specified string.
    /// </summary>
    /// <param name="search">The string to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice begins with <paramref name="search"/>; otherwise, false.</returns>
    public bool StartsWith(string search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().StartsWith(search, comparisonType);

    /// <summary>
    /// Determines whether this slice starts with the specified Value.
    /// </summary>
    /// <param name="search">The Value to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice begins with the characters in <paramref name="search"/>; otherwise, false.</returns>
    public bool StartsWith(StringSlice search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().StartsWith(search.AsSpan(), comparisonType);

    /// <summary>
    /// Determines whether this slice starts with the specified span of characters.
    /// </summary>
    /// <param name="search">The span of characters to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice begins with the characters in <paramref name="search"/>; otherwise, false.</returns>
    public bool StartsWith(ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().StartsWith(search, comparisonType);

    /// <summary>
    /// Determines whether this slice ends with the specified string.
    /// </summary>
    /// <param name="search">The string to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice ends with <paramref name="search"/>; otherwise, false.</returns>
    public bool EndsWith(string search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().EndsWith(search, comparisonType);

    /// <summary>
    /// Determines whether this slice ends with the specified StringSlice.
    /// </summary>
    /// <param name="search">The StringSlice to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice ends with the characters in <paramref name="search"/>; otherwise, false.</returns>
    public bool EndsWith(StringSlice search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().EndsWith(search.AsSpan(), comparisonType);

    /// <summary>
    /// Determines whether this slice ends with the specified span of characters.
    /// </summary>
    /// <param name="search">The span of characters to compare.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if this slice ends with the characters in <paramref name="search"/>; otherwise, false.</returns>
    public bool EndsWith(ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
        => this.AsSpan().EndsWith(search, comparisonType);

    /// <summary>
    /// Returns a new Value with all leading white-space characters removed.
    /// </summary>
    /// <returns>A new Value that starts with the first non-white-space character in this slice.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted starting position.
    /// White-space characters are defined by <see cref="char.IsWhiteSpace(char)"/>.
    /// </remarks>
    public StringSlice TrimStart() {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var span = this.Text.AsSpan(offset, length).TrimStart();
        var nextOffset = end - span.Length;
        return new StringSlice(this.Text, nextOffset..end);
    }

    /// <summary>
    /// Returns a new Value with all leading occurrences of the specified characters removed.
    /// </summary>
    /// <param name="chars">An array of characters to remove from the start.</param>
    /// <returns>A new Value that excludes the specified leading characters.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted starting position.
    /// </remarks>
    public StringSlice TrimStart(char[] chars) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var span = this.Text.AsSpan(offset, length).TrimStart(chars);
        var nextOffset = end - span.Length;
        return new StringSlice(this.Text, nextOffset..end);
    }

    /// <summary>
    /// Trims characters from the start of the slice based on a decision function.
    /// </summary>
    /// <param name="decide">A function that takes a character and its index and returns:
    /// 0 to continue trimming,
    /// less than 0 to return an empty slice,
    /// greater than 0 to stop trimming and return the remaining slice.</param>
    /// <returns>A new <see cref="StringSlice"/> with characters trimmed from the start according to the decision function.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted starting position.
    /// The decision function is called for each character from the start of the slice until it returns a non-zero value.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slice = new StringSlice("  123abc");
    /// // Trim until a digit is found
    /// var result = slice.TrimStart((c, i) => char.IsDigit(c) ? 1 : 0);
    /// Console.WriteLine(result.ToString()); // Outputs: "123abc"
    /// </code>
    /// </example>
    public StringSlice TrimWhile(Func<char, int, int> decide) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;

        if (length == 0) { return this; }
        for (int idx = offset; idx < end; idx++) {
            var decision = decide(this.Text[idx], idx - offset);
            if (decision == 0) {
                continue;
            } else if (decision < 0) {
                return new StringSlice(this.Text, idx..idx);
            } else if (decision > 0) {
                if (idx == offset) {
                    return this;
                } else {
                    return new StringSlice(this.Text, idx..end);
                }
            }
        }
        return new StringSlice(this.Text, end..end);
    }

    /// <summary>
    /// Returns a new Value with all trailing white-space characters removed.
    /// </summary>
    /// <returns>A new Value that excludes trailing white-space characters.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted ending position.
    /// White-space characters are defined by <see cref="char.IsWhiteSpace(char)"/>.
    /// </remarks>
    public StringSlice TrimEnd() {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var span = this.Text.AsSpan(offset, length).TrimEnd();
        var nextEnd = offset + span.Length;
        return new StringSlice(this.Text, offset..nextEnd);
    }

    /// <summary>
    /// Returns a new Value with all trailing occurrences of the specified characters removed.
    /// </summary>
    /// <param name="chars">An array of characters to remove from the end.</param>
    /// <returns>A new Value that excludes the specified trailing characters.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with an adjusted ending position.
    /// </remarks>
    public StringSlice TrimEnd(char[] chars) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var span = this.Text.AsSpan(offset, length).TrimEnd(chars);
        var nextEnd = offset + span.Length;
        return new StringSlice(this.Text, offset..nextEnd);
    }

    /// <summary>
    /// Returns a new Value with all leading and trailing white-space characters removed.
    /// </summary>
    /// <returns>A new Value that excludes both leading and trailing white-space characters.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with adjusted starting and ending positions.
    /// White-space characters are defined by <see cref="char.IsWhiteSpace(char)"/>.
    /// </remarks>
    public StringSlice Trim() {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var span = this.Text.AsSpan(offset, length).TrimEnd();
        var nextEnd = offset + span.Length;
        span = span.TrimStart();
        var nextOffset = nextEnd - span.Length;
        return new StringSlice(this.Text, nextOffset..nextEnd);
    }

    /// <summary>
    /// Returns a new Value with all leading and trailing occurrences of the specified characters removed.
    /// </summary>
    /// <param name="chars">An array of characters to remove from both the start and end.</param>
    /// <returns>A new Value that excludes the specified leading and trailing characters.</returns>
    /// <remarks>
    /// The returned slice references the same underlying string but with adjusted starting and ending positions.
    /// </remarks>
    public StringSlice Trim(char[] chars) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;

        var span = this.Text.AsSpan(offset, length).TrimEnd(chars);
        var nextEnd = offset + span.Length;

        span = span.TrimStart(chars);
        var nextOffset = nextEnd - span.Length;

        return new StringSlice(this.Text, nextOffset..nextEnd);
    }

    /// <summary>
    /// Splits the Value into two parts based on specified separator and optional stop characters.
    /// </summary>
    /// <param name="arraySeparator">An array of characters that delimit the split.</param>
    /// <param name="arrayStop">Optional array of characters that stop the split operation when encountered.</param>
    /// <returns>A <see cref="SplitInto"/> containing two StringSlices: the part before the separator and the remaining part.</returns>
    /// <remarks>
    /// If stop characters are provided and found, the split only considers the text up to the first stop character.
    /// The second part of the split (tail) has any leading separator characters removed.
    /// </remarks>
    public SplitInto SplitInto(char[] arraySeparator, char[]? arrayStop = default) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;

        if (length == 0) {
            return new SplitInto(this, this);
        }

        int posStop;
        if (arrayStop is not null && arrayStop.Length > 0) {
            posStop = this.IndexOfAny(arrayStop);
            if (posStop >= 0) {
                end = offset + posStop;
            }
        }

        int posSep = this.IndexOfAny(arraySeparator, 0..(end - offset));
        if (posSep < 0) {
            return new SplitInto(
                new StringSlice(this.Text, new Range(offset, end)),
                new StringSlice(this.Text, end..end));
        } else {
            var endSep = offset + posSep;
            return new SplitInto(
                new StringSlice(this.Text, new Range(offset, endSep)),
                (new StringSlice(this.Text, endSep..end)).TrimStart(arraySeparator)
                );
        }
    }

    /// <summary>
    /// Split the string into two parts, 
    /// the first part is the string length the decide function returns not 0.
    /// The second part is the rest of the string if <paramref name="decide"/> returns greater than 0.
    /// The second part is empty if <paramref name="decide"/> returns less than 0.
    /// </summary>
    /// <param name="decide">a callback to decide to 
    ///     0 continue,
    ///     greater 0 to return 2 parts the part Found (length before) and the Tail,
    ///     less 0 to return 2 parts the part Found (length before) and an empty Tail.</param>
    /// <returns>2 SubStrings the Found and the Tail.</returns>
    public SplitInto SplitIntoWhile(Func<char, int, int> decide) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        if (length == 0) { return new SplitInto(this, this); }

        for (int idx = offset; idx < end; idx++) {
            var result = decide(this.Text[idx], idx);
            if (result == 0) {
                continue;
            } else if (result > 0) {
                return new SplitInto(
                    new StringSlice(this.Text, new Range(offset, idx)),
                    new StringSlice(this.Text, idx..end));
            } else if (result < 0) {
                return new SplitInto(
                    new StringSlice(this.Text, new Range(offset, idx)),
                    new StringSlice(this.Text, idx..idx));
            }
        }
        return new SplitInto(this, new StringSlice(this.Text, end..end));
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Value.
    /// </summary>
    /// <param name="obj">The object to compare with the current Value.</param>
    /// <returns>true if the specified object is equal to the current Value; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is StringSlice other) { return this.Equals(other); }
        if (obj is string text) { return this.Equals(text.AsSpan(), StringComparison.Ordinal); }
        return false;
    }

    /// <summary>
    /// Determines whether the specified Value is equal to the current Value using ordinal comparison.
    /// </summary>
    /// <param name="other">The Value to compare with the current Value.</param>
    /// <returns>true if the specified Value is equal to the current Value; otherwise, false.</returns>
    public bool Equals(StringSlice other) {
        var t = this.AsSpan();
        var o = other.AsSpan();
        if (t.Length != o.Length) { return false; }
        return t.StartsWith(o, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the specified string is equal to the current Value.
    /// </summary>
    /// <param name="other">The string to compare with the current Value.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if the specified string is equal to the current Value; otherwise, false.</returns>
    public bool Equals(string other, StringComparison comparisonType = StringComparison.Ordinal) {
        var t = this.AsSpan();
        var o = other.AsSpan();
        if (t.Length != o.Length) { return false; }
        return t.StartsWith(o, comparisonType);
    }

    /// <summary>
    /// Determines whether the specified Value is equal to the current Value using the specified comparison type.
    /// </summary>
    /// <param name="other">The Value to compare with the current Value.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if the specified Value is equal to the current Value; otherwise, false.</returns>
    public bool Equals(StringSlice other, StringComparison comparisonType = StringComparison.Ordinal) {
        if (ReferenceEquals(this.Text, other.Text)) {
            return this.Range.Equals(other.Range);
        } else {
            var t = this.AsSpan();
            var o = other.AsSpan();
            if (t.Length != o.Length) { return false; }
            return t.StartsWith(o, comparisonType);
        }
    }

    /// <summary>
    /// Determines whether the specified character span is equal to the current Value.
    /// </summary>
    /// <param name="other">The character span to compare with the current Value.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if the specified character span is equal to the current Value; otherwise, false.</returns>
    public bool Equals(ReadOnlySpan<char> other, StringComparison comparisonType = StringComparison.Ordinal) {
        var t = this.AsSpan();
        if (t.Length != other.Length) { return false; }
        return t.StartsWith(other, comparisonType);
    }

    /// <summary>
    /// Returns a hash code for the current Value.
    /// </summary>
    /// <returns>A hash code for the current Value.</returns>
    public override int GetHashCode() => string.GetHashCode(this.AsSpan());

    /// <summary>
    /// Returns an enumerator that iterates through the characters in the Value.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the characters.</returns>
    public CharEnumerator GetEnumerator() => new CharEnumerator(this.AsSpan());

    /// <summary>
    /// Creates a new Value with all occurrences of a specified character replaced with another character.
    /// </summary>
    /// <param name="from">The character to be replaced.</param>
    /// <param name="to">The character that replaces the original character.</param>
    /// <returns>A new Value with all occurrences of <paramref name="from"/> replaced with <paramref name="to"/>.</returns>
    public StringSlice Replace(char from, char to) {
        if (0 == this.Length) { return this; }
        if (from == to) { return this; }
        if (!this.Contains(from)) { return this; }

        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;
        var result = new char[length];
        for (int idx = offset; idx < end; idx++) {
            var c = this.Text[idx];
            result[idx - offset] = c == from ? to : c;
        }
        return new StringSlice(new string(result));
    }

    /// <summary>
    /// Returns a new Value containing characters that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">A function that accepts a character and its index and returns true to include the character, false otherwise.</param>
    /// <returns>A new Value containing the characters that satisfy the predicate.</returns>
    public StringSlice ReadWhile(Func<char, int, bool> predicate) {
        var (offset, length) = this.Range.GetOffsetAndLength(this.Text.Length);
        var end = offset + length;

        var found = false;
        for (int idx = offset; idx < end; idx++) {
            if (!predicate(this.Text[idx], idx)) {
                return new StringSlice(this.Text, new Range(offset, idx));
            }
            found = true;
        }
        if (found) {
            return this;
        } else {
            return new StringSlice(this.Text, new Range(offset, offset));
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the characters in the Value.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the characters.</returns>
    public ref struct CharEnumerator {
        private readonly ReadOnlySpan<char> _Span;
        private int _Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CharEnumerator(ReadOnlySpan<char> span) {
            _Span = span;
            _Index = -1;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            var index = _Index + 1;
            if (index < _Span.Length) {
                _Index = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value>The element in the collection at the current position of the enumerator.</value>
        public ref readonly char Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this._Span[_Index];
        }
    }

    /// <summary>
    /// Implicitly converts a string to a Value.
    /// </summary>
    /// <param name="value">The string to convert. If null, an empty string is used.</param>
    /// <returns>A new Value containing the entire string.</returns>
    public static implicit operator StringSlice(string? value)
        => new StringSlice(value ?? string.Empty);

    /// <summary>
    /// Determines whether two Value instances are equal using ordinal comparison.
    /// </summary>
    /// <param name="left">The first Value to compare.</param>
    /// <param name="right">The second Value to compare.</param>
    /// <returns>true if the StringSlices are equal; otherwise, false.</returns>
    public static bool operator ==(StringSlice left, StringSlice right) => left.Equals(right, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two Value instances are not equal using ordinal comparison.
    /// </summary>
    /// <param name="left">The first Value to compare.</param>
    /// <param name="right">The second Value to compare.</param>
    /// <returns>true if the StringSlices are not equal; otherwise, false.</returns>
    public static bool operator !=(StringSlice left, StringSlice right) => !(left.Equals(right, StringComparison.Ordinal));

    /// <summary>
    /// Explicitly converts a <see cref="MutableStringSlice"/> to a <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="slice">The mutable string slice to convert.</param>
    /// <returns>A new string slice with the same text and range.</returns>
    public static explicit operator StringSlice(MutableStringSlice slice) {
        return new StringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Creates a <see cref="MutableStringSlice"/> from this string slice.
    /// </summary>
    /// <remarks>
    /// This method creates a new <see cref="MutableStringSlice"/> that references the same underlying text
    /// and uses the same range as this string slice. The resulting <see cref="MutableStringSlice"/> 
    /// allows modifications to the range while maintaining the same text reference.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stringSlice = new Value("Hello World", 0..5);
    /// var mutableSlice = stringSlice.AsMutableStringSlice();
    /// Console.WriteLine(mutableSlice.ToString()); // Outputs: "Hello"
    /// </code>
    /// </example>
    /// <returns>A new <see cref="MutableStringSlice"/> with the same text and range as this string slice.</returns>
    public MutableStringSlice AsMutableStringSlice() => new MutableStringSlice(this.Text, this.Range);

    /// <summary>
    /// Explicitly converts a <see cref="ImmutableStringSlice"/> to a <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="slice">The mutable string slice to convert.</param>
    /// <returns>A new string slice with the same text and range.</returns>
    public static explicit operator StringSlice(ImmutableStringSlice slice) {
        return new StringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Creates an <see cref="ImmutableStringSlice"/> from this string slice.
    /// </summary>
    /// <remarks>
    /// This method creates a new <see cref="ImmutableStringSlice"/> that references the same underlying text
    /// and uses the same range as this string slice. The resulting <see cref="ImmutableStringSlice"/> 
    /// is a reference type that provides the same view of the text but with different mutability characteristics.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stringSlice = new Value("Hello World", 0..5);
    /// var immutableSlice = stringSlice.AsImmutableStringSlice();
    /// Console.WriteLine(immutableSlice.ToString()); // Outputs: "Hello"
    /// </code>
    /// </example>
    /// <returns>A new <see cref="ImmutableStringSlice"/> with the same text and range as this string slice.</returns>
    public ImmutableStringSlice AsImmutableStringSlice() => new ImmutableStringSlice(this.Text, this.Range);

    /// <summary>
    /// Determines whether two <see cref="StringSlice"/> instances are equal using the specified string comparison
    /// option.
    /// </summary>
    /// <param name="a">The first <see cref="StringSlice"/> to compare.</param>
    /// <param name="b">The second <see cref="StringSlice"/> to compare.</param>
    /// <param name="ordinal">A <see cref="StringComparison"/> value that specifies the rules for the comparison.</param>
    /// <returns><see langword="true"/> if the two <see cref="StringSlice"/> instances are equal according to the specified
    /// comparison option; otherwise, <see langword="false"/>.</returns>
    public static bool Equals(StringSlice a, StringSlice b, StringComparison ordinal) {
        return a.Equals(b, ordinal);
    }
}
