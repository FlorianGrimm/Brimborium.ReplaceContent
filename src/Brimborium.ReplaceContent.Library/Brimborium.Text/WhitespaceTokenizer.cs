namespace Brimborium.Text;

public enum WhitespaceToken {
    Word,
    Whitespace,
    Newline
}

public record struct WhitespaceTokenStringSlice(
    StringSlice Text,
    WhitespaceToken Kind
    ) : ITokenStringSlice<WhitespaceToken>;

public class WhitespaceTokenizer : Tokenizer<WhitespaceToken> {
    public override TokenizedText<WhitespaceToken> Tokenize(StringSlice input) {
        var result = new List<ITokenStringSlice<WhitespaceToken>>();
        if (input.IsEmpty) {
            return new TokenizedText<WhitespaceToken>(result);
        }

        // [ \t]+ whitespace 
        // [^ \t\r\n]+ word
        // \r\n newline
        // \r newline
        // \n newline
        for (var watchdog = input.GetOffsetAndLength().Length; !input.IsEmpty && (0 <= watchdog); watchdog--) {
            // \n newline
            if ('\n' == input[0]) {
                result.Add(new WhitespaceTokenStringSlice(input.Substring(0, 1), WhitespaceToken.Newline));
                input = input.Substring(1);
                continue;
            }
            // \r\n newline
            // \r newline
            if ('\r' == input[0]) {
                var inputNext = input.Substring(1);
                if(!inputNext.IsEmpty && '\n' == inputNext[0]) {
                    result.Add(new WhitespaceTokenStringSlice(input.Substring(0, 2), WhitespaceToken.Newline));
                    input = input.Substring(2);
                    continue;
                } else {
                    result.Add(new WhitespaceTokenStringSlice(input.Substring(0, 1), WhitespaceToken.Newline));
                    input = inputNext;
                    continue;
                }
            }

            // [ \t]+ whitespace 
            if ((' ' == input[0]) || ('\t' == input[0])) { 
                var inputNext = input.Substring(1);
                while (!inputNext.IsEmpty && ((' ' == inputNext[0]) || ('\t' == inputNext[0]))) {
                    inputNext = inputNext.Substring(1);
                }
                input.Left(inputNext);
                result.Add(new WhitespaceTokenStringSlice(input.Left(inputNext), WhitespaceToken.Whitespace));
                input = inputNext;
                continue;
            }
            // [^ \t\r\n]+ word
            {
                var inputNext = input;
                while (!inputNext.IsEmpty) { 
                    var c = inputNext[0];
                    if (('\r' == c) || ('\n' == c) || ('\t' == c) || (' '==c)) {
                        break;
                    }
                    inputNext = inputNext.Substring(1);
                }
                var (inputOffset, _) = input.GetOffsetAndLength();
                var (inputNextOffset, _) = inputNext.GetOffsetAndLength();
                result.Add(new WhitespaceTokenStringSlice(input.Substring(0, inputNextOffset - inputOffset), WhitespaceToken.Word));
                input = inputNext;
            }
        }

        return new TokenizedText<WhitespaceToken>(result);

#if false
        
        int posFirst = 0;
        int posCurrent = 0;
        WhitespaceToken whitespaceToken = WhitespaceToken.Whitespace;
        // state | character      | next state | consume | token
        // 0     | whitespace     | 1          |   true  |
        // 1     | whitespace     | 1          |   false |  add whitespace
        // 1     | not whitespace | 0
        // 0     | \n             | 0          |   true  |  add whitespace
        // 0     | \r             | 2          |   true  |
        // 2     | \n             | 0          |   true  |  add whitespace
        // 2     | ^\n            | 0          |   false |  add whitespace
        // 0     | word           | 3          |   true  |
        // 3     | word           | 3          |   true  |
        // 3     | ^word          | 0          |   false |  add word

        int state = 0;
        int length = input.Length;
        while (posCurrent < length) {
            char c= input[posCurrent];
            if ('\n' == c) {
                
                continue;
            }
            if ('\r' == c) { 
            }
            if (' ' == c || '\t' == c) {
            }
        }
        if (posCurrent > posFirst) {
            result.Add(new WhitespaceTokenStringSlice(input.Substring(posFirst, posCurrent - posFirst), whitespaceToken));
        }
#endif

    }
}
