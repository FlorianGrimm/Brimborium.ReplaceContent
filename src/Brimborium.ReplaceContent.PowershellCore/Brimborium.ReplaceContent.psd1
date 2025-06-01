@{
    RootModule = 'Brimborium.ReplaceContent.PowershellCore.dll'
    ModuleVersion = '1.0.0'
    GUID = '289fd2e2-e5c6-427d-be1e-868f5fe1c478'
    Author = 'Florian Grimm'
    CompanyName = ''
    Copyright = 'MIT License.'
    # Description = ''
    # PowerShellVersion = ''
    # PowerShellHostName = ''
    # PowerShellHostVersion = ''
    # DotNetFrameworkVersion = ''
    # CLRVersion = ''
    # ProcessorArchitecture = ''

    RequiredModules = @()
    RequiredAssemblies = @(
    )
    ScriptsToProcess = @()
    TypesToProcess = @()
    FormatsToProcess = @()
    NestedModules = @()
    FunctionsToExport = @()
    CmdletsToExport = @(
        "Show-ReplaceContent",
        "Update-ReplaceContent"
    )

    VariablesToExport = ''
    AliasesToExport = @()
    DscResourcesToExport = @()
    ModuleList = @()
    FileList = @()

    PrivateData = @{
        PSData = @{
            # 'Tags' wurde auf das Modul angewendet und unterstützt die Modulermittlung in Onlinekatalogen.
            # Tags = @()

            # Eine URL zur Lizenz für dieses Modul.
            # LicenseUri = ''

            # Eine URL zur Hauptwebsite für dieses Projekt.
            # ProjectUri = ''

            # Eine URL zu einem Symbol, das das Modul darstellt.
            # IconUri = ''

            # 'ReleaseNotes' des Moduls
            # ReleaseNotes = ''

        } # Ende der PSData-Hashtabelle
    } # Ende der PrivateData-Hashtabelle

    # HelpInfoURI = ''

    DefaultCommandPrefix = ''
}
