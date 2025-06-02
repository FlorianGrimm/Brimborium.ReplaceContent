
namespace Brimborium.Text;

public class Tokenizer<TKind> {
    public virtual TokenizedText<TKind> Tokenize(StringSlice input) {
        return new TokenizedText<TKind>(new List<ITokenStringSlice<TKind>>());
    }
}
