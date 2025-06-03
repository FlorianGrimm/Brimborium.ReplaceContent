Set-StrictMode -Version Latest

Import-Module 'Brimborium.ReplaceContent'

#
# <Content>
# Show-ReplaceContent:45
$replacements = @{
    "Copyright" = "Copyright (c) 2023 Contoso Ltd. All rights reserved."
    "Version" = "1.2.3"
}
Show-ReplaceContent -Directory ".\src" -Replacements $replacements
#
# </Content>
#
'- fini -'