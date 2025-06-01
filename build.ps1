param(
    [string]$Configuration = "Debug"
)

dotnet build --configuration $Configuration

dotnet test --configuration $Configuration

Import-Module platyPS

import-module '.\Output\Brimborium.ReplaceContent.psd1'

Update-MarkdownHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs
New-ExternalHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs -OutputPath .\src\Brimborium.ReplaceContent.PowershellCore\
