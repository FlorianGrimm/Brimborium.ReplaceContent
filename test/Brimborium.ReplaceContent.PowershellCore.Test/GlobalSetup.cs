// You can use attributes at the assembly level to apply to all tests in the assembly
[assembly: TUnit.Core.NotInParallel()]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace Brimborium.ReplaceContent;

public class GlobalHooks {
    //[Before(TestSession)]
    //public static void SetUp() {
    //    Console.WriteLine(@"Or you can define methods that do stuff before...");
    //}

    //[After(TestSession)]
    //public static void CleanUp() {
    //    Console.WriteLine(@"...and after!");
    //}
}
