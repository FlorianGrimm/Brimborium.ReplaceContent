using Brimborium.Text;

namespace Brimborium.ReplaceContent;

public class RCParserTests {
    [Test]
    public async Task ParseWith1Placeholder() {
        var content = """
            aaaaaaaaa
            /* <Placeholder TestPlaceholder> */
            bbbbbbbbb
            /* </Placeholder TestPlaceholder> */
            ccccccccc
            """;
        var result = RCParser.Parse(content, "/*", "*/");
        
        await Assert.That(result.ListPart.Count).IsEqualTo(5);
        await Assert.That(result.IsValid).IsEqualTo(true);

        var settings = new VerifySettings();
        settings.IgnoreMembers<RCPart>(x => x.NextContent);
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(result, settings);
    }

    [Test]
    public async Task ParseWith1PlaceholderAndIndent() {
        var content = """
            aaaaaaaaa
                /* <Placeholder TestPlaceholder> */
                    bbbbbbbbb
                /* </Placeholder TestPlaceholder> */
            ccccccccc
            """;
        var result = RCParser.Parse(content, "/*", "*/");

        //await Assert.That(result.ListPart.Count).IsEqualTo(5);

        var settings = new VerifySettings();
        settings.IgnoreMembers<RCPart>(x => x.NextContent);
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(result, settings);
    }

    [Test]
    public async Task Parse_NoPlaceholder() {
        var content = """
            aaaaaaaaa
            /* xx */
            bbbbbbbbb
            /* xx */
            ccccccccc
            """;
        var result = RCParser.Parse(content, "/*", "*/");

        await Assert.That(result.ListPart.Count).IsEqualTo(1);

        var settings = new VerifySettings();
        settings.IgnoreMembers<RCPart>(x => x.NextContent);
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(result, settings);
    }


    [Test]
    public async Task Parse_CommentStartNotClosed() {
        var content = """
            aaaaaaaaa
            /* <Placeholder TestPlaceholder>
            bbbbbbbbb
            /* </Placeholder TestPlaceholder> */
            ccccccccc
            """;
        var result = RCParser.Parse(content, "/*", "*/");

        var settings = new VerifySettings();
        settings.IgnoreMembers<RCPart>(x => x.NextContent);
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(result, settings);
    }


    [Test]
    public async Task Parse_CommentEndNotClosed() {
        var content = """
            aaaaaaaaa
            /* <Placeholder TestPlaceholder> */
            bbbbbbbbb
            /* </Placeholder TestPlaceholder>
            ccccccccc
            """;
        var result = RCParser.Parse(content, "/*", "*/");

        await Assert.That(result.ContainsError(out var error)).IsTrue();
        await Assert.That(result.ListPart.Count).IsEqualTo(1);
        var settings = new VerifySettings();
        settings.IgnoreMembers<RCPart>(x => x.NextContent);
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(result, settings);
    }
}
