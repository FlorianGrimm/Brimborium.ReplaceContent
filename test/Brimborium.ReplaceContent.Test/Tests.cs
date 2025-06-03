using Brimborium.ReplaceContent.Test.Data;

namespace Brimborium.ReplaceContent.Test;
public class Tests {
    [Test]
    public async Task Test001() {
        var arrangePath = @"D:\github.com\FlorianGrimm\Brimborium.ReplaceContent\test\Brimborium.ReplaceContent.Test\Test001\Arrange";
        var actPath = @"D:\github.com\FlorianGrimm\Brimborium.ReplaceContent\test\Brimborium.ReplaceContent.Test\Test001\Act";
        var assertPath = @"D:\github.com\FlorianGrimm\Brimborium.ReplaceContent\test\Brimborium.ReplaceContent.Test\Test001\Assert";
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

        // compare files in Act and Assert directories
        foreach (var fi in diAct.GetFiles()) {
            var fileName = System.IO.Path.Combine(diAssert.FullName, fi.Name);
            if (System.IO.File.Exists(fileName)) {
                var contentAct = await System.IO.File.ReadAllTextAsync(fi.FullName);
                var contentAssert = await System.IO.File.ReadAllTextAsync(fileName);
                await Assert.That(contentAct).IsEqualTo(contentAssert);
            } else {
                Assert.Fail($"File {fi.Name} does not exist in Assert directory.");
            }
        }
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