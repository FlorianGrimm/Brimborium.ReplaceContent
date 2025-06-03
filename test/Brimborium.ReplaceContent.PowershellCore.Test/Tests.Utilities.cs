using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.ReplaceContent.Library;

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

    private TestResult RunPowershellTest([CallerMemberName] string testName = "") {
        if (string.IsNullOrEmpty(testName)) { throw new ArgumentException(nameof(testName)); }

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
        }

        // ensure CurrentDirectory is projectPath
        if (!string.Equals(System.Environment.CurrentDirectory, projectPath, StringComparison.OrdinalIgnoreCase)) {
            System.Environment.CurrentDirectory = projectPath;
        }
        bool success = false;
        List<object> listOutput = new();
        List<object> listError = new();

        using (var powershell = System.Management.Automation.PowerShell.Create(
            System.Management.Automation.RunspaceMode.NewRunspace)) {
            powershell.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process");
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
}
