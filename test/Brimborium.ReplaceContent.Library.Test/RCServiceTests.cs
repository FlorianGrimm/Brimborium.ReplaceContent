namespace Brimborium.ReplaceContent;

public class RCServiceTests {
    [Test]
    public async Task Integration001() {
        var rcService = new RCService();
        var context = rcService.NewContext();
        rcService.InitializeFileTypeByExtension(context);
        rcService.AddPlaceholder(context, "TestPlaceholder", "TestReplacement");
        var content = rcService.AddContentText(
            context, 
            "TestFile.txt",
            """
            aaaaaaaaa
              /* <Placeholder TestPlaceholder> */
                bbbbbbbbb
              /* </Placeholder TestPlaceholder> */
            ccccccccc
            """);
        rcService.SetFileType(context, content);
        rcService.Scan(context, content);
        rcService.Replace(context, content);
        rcService.GenerateNextContent(context, content);

        var settings = new VerifySettings();
        settings.IgnoreMembers<StringSlice>(x => x.Range);
        settings.IgnoreMembers<StringSlice>(x => x.Length);
        await Verify(content, settings);
    }
}
