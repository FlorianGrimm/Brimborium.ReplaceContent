using System.Threading.Tasks;

using Brimborium.ReplaceContent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

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
    public static int Main(string[] args) {
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

        var appParameters = new AppParameters();
        configuration.Bind(appParameters);

        var replaceContentService = serviceProvider.GetRequiredService<RCService>();
        var context = replaceContentService.NewContext();
        replaceContentService.Initialize(context);
        
        if (appParameters.FileType is { Count: > 0 } appFileType) { 
            foreach (var kvp in appFileType) {
                context.FileTypeByExtension[kvp.Key] = kvp.Value;
            }
        } else 

        replaceContentService.AddPlaceholderDirectory(context, appParameters.ReplacementsDirectory);
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
            replaceContentService.ShowDiff(context);
        } else {
            replaceContentService.Write(context);
        }
        return result;
    }
}
