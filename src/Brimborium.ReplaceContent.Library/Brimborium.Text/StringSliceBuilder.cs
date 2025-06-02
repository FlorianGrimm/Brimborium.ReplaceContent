namespace Brimborium.Text;

/// <summary>
/// Provides a way to efficiently build strings by concatenating multiple <see cref="StringSlice"/> instances.
/// This class is designed to minimize memory allocations during string concatenation operations.
/// </summary>
/// <remarks>
/// The builder maintains a list of string slices and only performs the actual string concatenation
/// when <see cref="ToString"/> is called, using a pooled StringBuilder for efficiency.
/// </remarks>
public sealed class StringSliceBuilder {
    private readonly List<StringSlice> _Slices = new List<StringSlice>();

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSliceBuilder"/> class.
    /// </summary>
    public StringSliceBuilder() {
    }

    /// <summary>
    /// Appends a string to the builder by converting it to a <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>The current instance to enable method chaining.</returns>
    public StringSliceBuilder Append(string value) => this.Append(value.AsStringSlice());

    /// <summary>
    /// Appends a <see cref="StringSlice"/> to the builder.
    /// </summary>
    /// <param name="value">The string slice to append.</param>
    /// <returns>The current instance to enable method chaining.</returns>
    /// <remarks>
    /// Empty slices are ignored to optimize memory usage and performance.
    /// </remarks>
    public StringSliceBuilder Append(StringSlice value) {
        if (value.IsEmpty) {
            return this;
        }
        this._Slices.Add(value);
        return this;
    }

    /// <summary>
    /// Concatenates all the stored string slices and returns the resulting string.
    /// </summary>
    /// <returns>A string containing all the appended content.</returns>
    /// <remarks>
    /// This method uses a pooled StringBuilder to efficiently concatenate the strings,
    /// minimizing memory allocations. The StringBuilder is returned to the pool after use.
    /// </remarks>
    override public string ToString() {
        if (this._Slices.Count == 0) {
            return string.Empty;
        }
        var length = 0;
        foreach (var slice in this._Slices) {
            length += slice.Length;
        }
        var sb = StringBuilderPool.Instance.Get();
        if (sb.Capacity < length) {
            sb.EnsureCapacity(length);
        }
        foreach (var slice in this._Slices) {
            sb.Append(slice.ToString());
        }
        var result = sb.ToString();
        StringBuilderPool.Instance.Return(sb);
        return result;
    }
}
