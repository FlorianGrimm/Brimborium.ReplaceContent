namespace Brimborium.Text;

public enum NewlineToken {
    Word,
    Newline
}

public record struct NewlineTokenStringSlice(
    StringSlice Text,
    NewlineToken Kind
    ) : ITokenStringSlice<NewlineToken>;

/// <summary>
/// Tokenizes a given input text into newline and other (word) tokens.
/// </summary>
/// <remarks>This tokenizer processes the input text by identifying newline characters (\r  or \n)  and
/// grouping all other characters into word tokens. Each token is represented as a slice of the  input string with an
/// associated token type.</remarks>
public class NewlineTokenizer : Tokenizer<NewlineToken> {
    private static NewlineTokenizer? _Instance;
    public static NewlineTokenizer Instance => (_Instance ??= new NewlineTokenizer());

    public override TokenizedText<NewlineToken> Tokenize(StringSlice input) {
        var result = new List<ITokenStringSlice<NewlineToken>>();
        if (input.IsEmpty) {
            return new TokenizedText<NewlineToken>(result);
        }
        for (var watchdog = input.GetOffsetAndLength().Length; !input.IsEmpty && (0 <= watchdog); watchdog--) {
            // \n newline
            if ('\n' == input[0]) {
                result.Add(new NewlineTokenStringSlice(input.Substring(0, 1), NewlineToken.Newline));
                input = input.Substring(1);
                continue;
            }
            // \r\n newline
            // \r newline
            if ('\r' == input[0]) {
                var inputNext = input.Substring(1);
                if (!inputNext.IsEmpty && '\n' == inputNext[0]) {
                    result.Add(new NewlineTokenStringSlice(input.Substring(0, 2), NewlineToken.Newline));
                    input = input.Substring(2);
                    continue;
                } else {
                    result.Add(new NewlineTokenStringSlice(input.Substring(0, 1), NewlineToken.Newline));
                    input = inputNext;
                    continue;
                }
            }
            // [^\r\n]+ word
            {
                var inputNext = input.Substring(1);
                while (!inputNext.IsEmpty && (('\n' != inputNext[0]) && ('\r' != inputNext[0]))) {
                    inputNext = inputNext.Substring(1);
                }
                result.Add(new NewlineTokenStringSlice(input.Left(inputNext), NewlineToken.Word));
                input = inputNext;
                continue;
            }
        }

        return new TokenizedText<NewlineToken>(result);
    }
}