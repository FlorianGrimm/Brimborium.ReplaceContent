namespace Brimborium.Text;

/// <summary>
/// The result of a Tokenizer
/// </summary>
/// <typeparam name="TToken">The token (enum?)</typeparam>
public class TokenizedText<TToken> {
    private readonly List<ITokenStringSlice<TToken>> _ListTokens;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="listTokens"></param>
    public TokenizedText(List<ITokenStringSlice<TToken>> listTokens) {
        this._ListTokens = listTokens;
    }

    /// <summary>
    /// 
    /// </summary>
    public List<ITokenStringSlice<TToken>> ListTokens => this._ListTokens;
}
