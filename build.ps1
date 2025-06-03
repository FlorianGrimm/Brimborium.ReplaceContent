param(
    [string]$Configuration = "Debug"
)

dotnet build --configuration $Configuration

if ($Configuration -eq "Debug") {
    Import-Module platyPS
    import-module '.\Output\Brimborium.ReplaceContent.psd1'
    Update-MarkdownHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs
    
    # New-ExternalHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs -OutputPath .\src\Brimborium.ReplaceContent.PowershellCore\ -Force
}

if ($Configuration -eq "Debug") {
    dotnet test "test\Brimborium.ReplaceContent.PowershellCore.Test\Brimborium.ReplaceContent.PowershellCore.Test.csproj" --filter "FullyQualifiedName~Brimborium.ReplaceContent.Library.TestPrepares"
    dotnet test "test\Brimborium.ReplaceContent.PowershellCore.Test\Brimborium.ReplaceContent.PowershellCore.Test.csproj" --filter "FullyQualifiedName~Brimborium.ReplaceContent.Library.Tests"
}

if ($Configuration -eq "Release") {

    dotnet test --configuration Release

    dotnet publish src/Brimborium.ReplaceContent/Brimborium.ReplaceContent.csproj --configuration $Configuration --output Package

    mkdir ./assets -ErrorAction SilentlyContinue | out-null

    Compress-Archive -Path .\Package\ -DestinationPath "./assets/Brimborium.ReplaceContent-$([System.DateTime]::Today.ToString('yyyy-mm-dd')).zip"
}
