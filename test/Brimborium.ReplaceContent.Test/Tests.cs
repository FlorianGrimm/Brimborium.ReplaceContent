using System.Runtime.CompilerServices;

namespace Brimborium.ReplaceContent.Test;

public class Tests {
    [Test]
    public async Task Test001() {
        var testPath = System.IO.Path.Combine(GetProjectDirectory(), "Test001");
        await RunTest(testPath);
    }

    [Test]
    public async Task Test002() {
        var testPath = System.IO.Path.Combine(GetProjectDirectory(), "Test002");
        await RunTest(testPath);
    }

    private static async Task RunTest(string testPath) {
        var arrangePath = System.IO.Path.Combine(testPath, "Arrange");
        var actPath = System.IO.Path.Combine(testPath, "Act");
        var assertPath = System.IO.Path.Combine(testPath, "Assert");

        System.IO.DirectoryInfo diArrange = new System.IO.DirectoryInfo(arrangePath);
        System.IO.DirectoryInfo diAct = new System.IO.DirectoryInfo(actPath);
        System.IO.DirectoryInfo diAssert = new System.IO.DirectoryInfo(assertPath);

        // create the Assert directory if it does not exist
        if (!diAct.Exists) { diAct.Create(); }

        // copy files from Arrange to Act directory
        foreach (var di in diArrange.GetDirectories()) {
            diAct.CreateSubdirectory(di.Name);
        }
        var arrangeFullName = diArrange.FullName;
        foreach (var fi in diArrange.GetFiles("*.*", new EnumerationOptions() { RecurseSubdirectories = true })) {
            if (fi.FullName.StartsWith(arrangeFullName)) {
                var relativePath = fi.FullName.Substring(arrangeFullName.Length).TrimStart('\\', '/');
                fi.CopyTo(System.IO.Path.Combine(diAct.FullName,  relativePath), true);
            }
        }

        // invoke tool
        AppParameters appParameters = new AppParameters {
            Directory = diAct.FullName,
            ReplacementsDirectory = System.IO.Path.Combine(diAct.FullName, "Replacements"),
            Write = true,
            Verbose = true
        };

        await Brimborium.ReplaceContent.Program.Run(new string[] { }, appParameters);

        // compare files in Act and Assert directories
        foreach (var fiAssert in diAssert.GetFiles()) {
            var fileNameAct = System.IO.Path.Combine(diAct.FullName, fiAssert.Name);
            if (System.IO.File.Exists(fileNameAct)) {
                var contentAct = await System.IO.File.ReadAllTextAsync(fileNameAct);
                var contentAssert = await System.IO.File.ReadAllTextAsync(fiAssert.FullName);
                await Assert.That(contentAct).IsEqualTo(contentAssert);
            } else {
                Assert.Fail($"File {fiAssert.Name} does not exist in Assert directory.");
            }
        }
    }

    private static string GetProjectDirectory([CallerFilePath] string callerFilePath = "") {
        return System.IO.Path.GetDirectoryName(callerFilePath) ?? throw new ArgumentException(nameof(callerFilePath));
    }
}
