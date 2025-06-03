Set-StrictMode -Version Latest

Import-Module 'Brimborium.ReplaceContent'

#
# <Content>
# Update-ReplaceContent:45
$replacements = @{
    "Copyright" = "Copyright (c) 2023 Contoso Ltd. All rights reserved."
    "Version" = "1.2.3"
}
Update-ReplaceContent -Directory ".\src" -Replacements $replacements
#
# </Content>
#
'- fini -'