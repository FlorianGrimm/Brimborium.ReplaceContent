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

    [Test]
    public async Task Integration_Customize_UsesCustomizedContent()
    {
        var rcService = new RCService();
        var context = rcService.NewContext();
        rcService.InitializeFileTypeByExtension(context);

        // No replacement is added, so customized content should be used
        var content = rcService.AddContentText(
            context,
            "TestFileCustomize.txt",
            """
            aaaaaaaaa
            /* <Default TestName> */
            default-content
            /* </Default TestName> */
            /* <Customize TestName> */
            customized-content
            /* </Customize TestName> */
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

    [Test]
    public async Task Integration_Customize_UsesDefaultIfNoCustomizedContent()
    {
        var rcService = new RCService();
        var context = rcService.NewContext();
        rcService.InitializeFileTypeByExtension(context);

        // No replacement is added, and customize block is empty, so default should be used
        var content = rcService.AddContentText(
            context,
            "TestFileCustomizeDefault.txt",
            """
            aaaaaaaaa
            /* <Default TestName> */
            default-content
            /* </Default TestName> */
            /* <Customize TestName> */
            /* </Customize TestName> */
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


    [Test]
    public async Task Integration_Customize_UpdateDefaultIfNoCustomizedContent() {
        var rcService = new RCService();
        var context = rcService.NewContext();
        rcService.InitializeFileTypeByExtension(context);
        context.DictReplacement.AddString("TestName", "new-default-content");

        // No replacement is added, and customize block is empty, so default should be used
        var content = rcService.AddContentText(
            context,
            "TestFileCustomizeDefault.txt",
            """
            aaaaaaaaa
            /* <Default TestName> */
            default-content
            /* </Default TestName> */
            /* <Customize TestName> */
            /* </Customize TestName> */
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
