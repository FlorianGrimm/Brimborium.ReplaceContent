namespace Brimborium.ReplaceContent;

public partial class Tests {
    internal record class TestResult(
        bool Success,
        List<object> ListOutput,
        List<object> ListError
        ) {
        public TestResult FilterOutput(Func<List<object>, List<object>> filter) {
            var nextListOutput = filter(this.ListOutput);
            return this with {
                ListOutput = nextListOutput
            };
        }
    }

    private async ValueTask<TestResult> RunPowershellTest([CallerMemberName] string testName = "") {
        if (string.IsNullOrEmpty(testName)) { throw new ArgumentException(nameof(testName)); }

        var solutionPath = GetSolutionRoot();
        var outputModulePath = System.IO.Path.Combine(solutionPath, "output", "Brimborium.ReplaceContent.psd1");
        var projectPath = GetProjectPath();

        // load
        string powershellContent;
        {
            if (testName is not { Length: > 0 }) {
                throw new ArgumentException(nameof(testName));
            }
            var fileName = testName.EndsWith(".ps1")
                ? testName
                : $"Tests.{testName}.ps1";
            var filePath = System.IO.Path.Combine(projectPath, fileName);
            if (!System.IO.File.Exists(filePath)) {
                throw new FileNotFoundException(filePath);
            }
            powershellContent = System.IO.File.ReadAllText(filePath);
            powershellContent = powershellContent.Replace("Set-StrictMode -Version Latest", "").Replace("Import-Module 'Brimborium.ReplaceContent'", "");
        }

        // ensure CurrentDirectory is projectPath
        var testFolderName = testName.EndsWith(".ps1")
            ? testName.Substring(0, testName.Length-4)
            : $"Tests.{testName}";

        var testPath = System.IO.Path.Combine(projectPath, testFolderName);
        if (System.IO.Directory.Exists(testPath)) {
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
                    fi.CopyTo(System.IO.Path.Combine(diAct.FullName, relativePath), true);
                }
            }
            if (!string.Equals(System.Environment.CurrentDirectory, actPath, StringComparison.OrdinalIgnoreCase)) {
                System.Environment.CurrentDirectory = actPath;
            }

        } else {
            if (!string.Equals(System.Environment.CurrentDirectory, projectPath, StringComparison.OrdinalIgnoreCase)) {
                System.Environment.CurrentDirectory = projectPath;
            }
        }

        bool success = false;
        List<object> listOutput = new();
        List<object> listError = new();

        using (var powershell = System.Management.Automation.PowerShell.Create(
            System.Management.Automation.RunspaceMode.NewRunspace)) {
            powershell.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process");
            powershell.AddScript("Set-StrictMode -Version Latest");
            powershell.AddScript($"Import-Module '{outputModulePath}'");
            powershell.AddScript(powershellContent);
            try {
                // Execute the powershell script
                var invokeResult = powershell.Invoke();

                // and read the result
                foreach (var result in invokeResult) {
                    if (result.ImmediateBaseObject is { } immediateBaseObject) {
                        listOutput.Add(immediateBaseObject);
                    }
                }
                success = true;
            } catch (System.Exception error) {
                listError.Add(error);
                success = false;
            }

            if (powershell.HadErrors) {
                success = false;
                var listStreamsError = powershell.Streams.Error.ToList();
                foreach (var errorRecord in listStreamsError) {
                    listError.Add(errorRecord.ToString());
                }
            }
        }
        if (success) {
            if (System.IO.Directory.Exists(testPath)) {
                var arrangePath = System.IO.Path.Combine(testPath, "Arrange");
                var actPath = System.IO.Path.Combine(testPath, "Act");
                var assertPath = System.IO.Path.Combine(testPath, "Assert");

                System.IO.DirectoryInfo diArrange = new System.IO.DirectoryInfo(arrangePath);
                System.IO.DirectoryInfo diAct = new System.IO.DirectoryInfo(actPath);
                System.IO.DirectoryInfo diAssert = new System.IO.DirectoryInfo(assertPath);


                // compare files in Act and Assert directories
                foreach (var fiAssert in diAssert.GetFiles()) {
                    var fileNameAct = System.IO.Path.Combine(diAct.FullName, fiAssert.Name);
                    if (System.IO.File.Exists(fileNameAct)) {
                        var contentAct = System.IO.File.ReadAllTextAsync(fileNameAct);
                        var contentAssert = await System.IO.File.ReadAllTextAsync(fiAssert.FullName);
                        await Assert.That(contentAct).IsEqualTo(contentAssert);
                    } else {
                        Assert.Fail($"File {fiAssert.Name} does not exist in Assert directory.");
                    }
                }
            }
        }
        return new TestResult(success, listOutput, listError);
    }

    private static string? _GetProjectPathCache;
    private static string GetProjectPath([CallerFilePath] string? callerFilePath = default) {
        if (_GetProjectPathCache is { } result) {
            return result;
        } else {
            result = System.IO.Path.GetDirectoryName(
                callerFilePath ?? throw new ArgumentException(nameof(callerFilePath))
                ) ?? throw new ArgumentException(nameof(callerFilePath));
            return _GetProjectPathCache = result;
        }
    }

    private static string GetSolutionRoot([CallerFilePath] string? callerFilePath = default) {
        var result = callerFilePath ?? string.Empty;
        for (int i = 0; (i < 3) && !string.IsNullOrEmpty(result); i++) {
            result = System.IO.Path.GetDirectoryName(result);
        }
        return result ?? string.Empty;
    }
}
