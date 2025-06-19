namespace Brimborium.ReplaceContent;

public class RCReplacementValue {
    public StringSlice? Value { get; set; }
    public RCPart? Part { get; set; }

    public StringSlice GetValue() {
        if (Value is { } value) { return value; }
        if (this.Part is { } part) { return part.OldContent; }
        return string.Empty;
    }
}

public class RCReplacementDictionary {
    public Dictionary<StringSlice, RCReplacementValue> Inner { get; set; } = new Dictionary<StringSlice, RCReplacementValue>(comparer:StringSliceComparer.OrdinalIgnoreCase);
    
    public void AddStringSlice(StringSlice key, StringSlice value) {
        this.Inner[key] = new RCReplacementValue() { Value = value };
    }
    public void AddString(StringSlice key, string value) {
        this.Inner[key] = new RCReplacementValue() { Value = value.AsStringSlice() };
    }

    public void AddPart(StringSlice key, RCPart value) {
        this.Inner[key] = new RCReplacementValue() { Part = value };
    }

    public void Add(Dictionary<string, string> value) {
        foreach (var (k, v) in value) {
            this.AddStringSlice(k.AsStringSlice(), v.AsStringSlice());
        }
    }

    public bool TryGetValue(StringSlice placeholderName, [MaybeNullWhen(false)] out RCReplacementValue replacement) {
        return this.Inner.TryGetValue(placeholderName, out replacement);
    }
}
