namespace Brimborium.Text;

public class StringSliceComparer(StringComparison stringComparison)
    : IComparer<StringSlice>
    , IEqualityComparer<StringSlice> {

    private static StringSliceComparer? _Ordinal;
    private static StringSliceComparer? _OrdinalIgnoreCase;
    private static StringSliceComparer? _InvariantCulture;
    private static StringSliceComparer? _InvariantCultureIgnoreCase;
    public static StringSliceComparer Ordinal => _Ordinal ??= new(StringComparison.Ordinal);
    public static StringSliceComparer OrdinalIgnoreCase => _OrdinalIgnoreCase ??= new(StringComparison.OrdinalIgnoreCase);
    public static StringSliceComparer InvariantCulture => _InvariantCulture ??= new(StringComparison.InvariantCulture);
    public static StringSliceComparer InvariantCultureIgnoreCase => _InvariantCultureIgnoreCase ??= new(StringComparison.InvariantCultureIgnoreCase);

    private readonly StringComparison _StringComparison = stringComparison;

    public int Compare(StringSlice x, StringSlice y)
        => x.AsSpan().CompareTo(y.AsSpan(), this._StringComparison);

    public bool Equals(StringSlice x, StringSlice y)
        => x.AsSpan().Equals(y.AsSpan(), this._StringComparison);

    public int GetHashCode([DisallowNull] StringSlice obj)
        => string.GetHashCode(obj.AsSpan(), _StringComparison);
}
