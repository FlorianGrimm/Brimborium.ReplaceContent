using DiffEngine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brimborium.ReplaceContent;

public class AppParameters {
    public string? Directory { get; set; }
    public string? File { get; set; }
    public string ReplacementsDirectory { get; set; } = "Replacements";
    public string? FileExtensions { get; set; }
    public bool Write { get; set; } = false;
    public bool Verbose { get; set; } = false;

    public Dictionary<string, RCFileType> FileType = new();
}

public class Program {
    public static async Task<int> Main(string[] args) {
        var appParameters = new AppParameters();
        return await Program.Run(args, appParameters).ConfigureAwait(false);
    }
    public static async Task<int> Run(string[] args, AppParameters appParameters) {
        var configurationBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
        configurationBuilder.AddCommandLine(args);
        configurationBuilder.AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();
        if (configuration.GetSection("Configuration").Value is { Length: > 0 } configurationValue) {
            configurationBuilder.AddJsonFile(configurationValue, optional: true, reloadOnChange: true);
            configuration = configurationBuilder.Build();
        }

        var servicesBuilder = new ServiceCollection();
        servicesBuilder.AddLogging(loggingBuilder => {
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            if (configuration.GetValue<bool>("Verbose")) {
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            } else {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            }
            loggingBuilder.AddConsole();
        });
        servicesBuilder.AddSingleton<RCService>();
        var serviceProvider = servicesBuilder.BuildServiceProvider();

        configuration.Bind(appParameters);

        var replaceContentService = serviceProvider.GetRequiredService<RCService>();
        var context = replaceContentService.NewContext();

        if (appParameters.FileType is { Count: > 0 } appFileType) {
            foreach (var kvp in appFileType) {
                context.FileTypeByExtension[kvp.Key] = kvp.Value;
            }
        } else { 
            replaceContentService.InitializeFileTypeByExtension(context);
        }

        replaceContentService.AddPlaceholderDirectory(context, appParameters.ReplacementsDirectory, true);

        if (appParameters.File is null && appParameters.Directory is null) {
            var locationDirectory = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var currentDirectory = System.Environment.CurrentDirectory;
            if (string.Equals(locationDirectory, currentDirectory, StringComparison.OrdinalIgnoreCase)) {
                // do nothing
            } else {
                appParameters.Directory = currentDirectory;
            }
        }

        int result = 0;
        if (appParameters.File is { Length: > 0 } filePath) {
            replaceContentService.AddContentFile(context, filePath);
        }
        if (appParameters.Directory is { Length: > 0 } directoryPath) {
            if (appParameters.FileExtensions is { Length: > 0 } fileExtensions && ".*" != fileExtensions) {
                var fileExtensionsSet = new HashSet<string>(fileExtensions.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
                replaceContentService.AddContentDirectory(context, directoryPath, fileExtensionsSet);
            } else {
                replaceContentService.AddContentDirectory(context, directoryPath, null);
            }
        }
        replaceContentService.Scan(context);
        replaceContentService.Replace(context);
        if (!appParameters.Write) {
            if (!DiffEngine.DiffRunner.Disabled) {
                var (listDiff, _) = replaceContentService.GetDiffFile(context);
                if (listDiff.Count == 0) {
                    Console.WriteLine("No differences found.");
                    return 0;
                }
                if (appParameters.Verbose) {
                    Console.WriteLine("Differences found:");
                    foreach (var diff in listDiff) {
                        Console.WriteLine($"- {diff.FilePath}");
                    }
                }
                foreach (var diff in listDiff) {
                    if (diff.NextFilePath is null || diff.FilePath is null) {
                        continue;
                    }
                    var launchResult = await DiffEngine.DiffRunner.LaunchForTextAsync(
                        tempFile: diff.NextFilePath,
                        targetFile: diff.FilePath);
                    switch (launchResult) {
                        case LaunchResult.NoEmptyFileForExtension:
                            break;
                        case LaunchResult.AlreadyRunningAndSupportsRefresh:
                            break;
                        case LaunchResult.StartedNewInstance:
                            break;
                        case LaunchResult.TooManyRunningDiffTools:
                            return 0;
                        case LaunchResult.NoDiffToolFound:
                            return 0;
                        case LaunchResult.Disabled:
                            return 0;
                        default:
                            break;
                    }
                }
                return 0;
            }
            {
                replaceContentService.ShowDiff(context);
            }
        } else {
            replaceContentService.Write(context);
        }
        return result;
    }
}
