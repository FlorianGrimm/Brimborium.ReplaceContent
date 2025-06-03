using System.Runtime.CompilerServices;

namespace Brimborium.ReplaceContent.Test;

public class Tests {
    [Test]
    public async Task Test001() {
        var testPath=System.IO.Path.Combine(GetProjectDirectory(), "Test001");
        var arrangePath = System.IO.Path.Combine(testPath, "Arrange");
        var actPath = System.IO.Path.Combine(testPath, "Act");
        var assertPath = System.IO.Path.Combine(testPath, "Assert");

        System.IO.DirectoryInfo diArrange = new System.IO.DirectoryInfo(arrangePath);
        System.IO.DirectoryInfo diAct = new System.IO.DirectoryInfo(actPath);
        System.IO.DirectoryInfo diAssert = new System.IO.DirectoryInfo(assertPath);

        // create the Assert directory if it does not exist
        if (!diAct.Exists) { diAct.Create(); }

        // copy files from Arrange to Act directory
        foreach (var di in diAct.GetDirectories()) {
            diAct.CreateSubdirectory(di.Name);
        }
        foreach (var fi in diArrange.GetFiles()) {
            fi.CopyTo(System.IO.Path.Combine(diAct.FullName, fi.Name), true);
        }

        // invoke tool
        AppParameters appParameters=new AppParameters {
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

    /*
        [Test]
        public void Basic() {
            Console.WriteLine("This is a basic test");
        }

        [Test]
        [Arguments(1, 2, 3)]
        [Arguments(2, 3, 5)]
        public async Task DataDrivenArguments(int a, int b, int c) {
            Console.WriteLine("This one can accept arguments from an attribute");

            var result = a + b;

            await Assert.That(result).IsEqualTo(c);
        }

        [Test]
        [MethodDataSource(nameof(DataSource))]
        public async Task MethodDataSource(int a, int b, int c) {
            Console.WriteLine("This one can accept arguments from a method");

            var result = a + b;

            await Assert.That(result).IsEqualTo(c);
        }

        [Test]
        [ClassDataSource<DataClass>]
        [ClassDataSource<DataClass>(Shared = SharedType.PerClass)]
        [ClassDataSource<DataClass>(Shared = SharedType.PerAssembly)]
        [ClassDataSource<DataClass>(Shared = SharedType.PerTestSession)]
        public void ClassDataSource(DataClass dataClass) {
            Console.WriteLine("This test can accept a class, which can also be pre-initialised before being injected in");

            Console.WriteLine("These can also be shared among other tests, or new'd up each time, by using the `Shared` property on the attribute");
        }

        [Test]
        [DataGenerator]
        public async Task CustomDataGenerator(int a, int b, int c) {
            Console.WriteLine("You can even define your own custom data generators");

            var result = a + b;

            await Assert.That(result).IsEqualTo(c);
        }

        public static IEnumerable<(int a, int b, int c)> DataSource() {
            yield return (1, 1, 2);
            yield return (2, 1, 3);
            yield return (3, 1, 4);
        }
    */
}