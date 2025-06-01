<#
pwsh -f .\install-deps.ps1
#>

<#
https://github.com/PowerShell/platyPS
#>
Install-Module -Name platyPS -Scope CurrentUser
Import-Module platyPS

Install-Module -Name dbatools -Scope CurrentUser
Import-Module dbatools

<#
$m=import-module '.\Output\Brimborium.ReplaceContent.psd1' -PassThru
$m|fl

New-MarkdownHelp -Module 'Brimborium.ReplaceContent' -OutputFolder .\src\Brimborium.ReplaceContent.PowershellCore\docs
New-ExternalHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs -OutputPath .\src\Brimborium.ReplaceContent.PowershellCore\help\

# re-import your module with latest changes
import-module '.\Output\Brimborium.ReplaceContent.psd1'
Update-MarkdownHelp .\src\Brimborium.ReplaceContent.PowershellCore\docs
#>
