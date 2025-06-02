namespace Brimborium.Text;

/// <summary>
/// The token 
/// </summary>
/// <typeparam name="TToken">The token (enum?)</typeparam>
public interface ITokenStringSlice<TToken> {
    StringSlice Text { get; set; }
    TToken Kind { get; set; }
}

/// <summary>
/// The simplest Token
/// </summary>
/// <typeparam name="TToken">The token (enum?)</typeparam>
public record struct TokenStringSlice<TToken>(
    StringSlice Text,
    TToken Kind) : ITokenStringSlice<TToken> {
}
