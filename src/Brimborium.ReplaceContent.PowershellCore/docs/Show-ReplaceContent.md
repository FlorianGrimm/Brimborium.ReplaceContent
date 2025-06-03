---
external help file: Brimborium.ReplaceContent.PowershellCore.dll-Help.xml
Module Name: Brimborium.ReplaceContent
online version:
schema: 2.0.0
---

# Show-ReplaceContent

## SYNOPSIS
Displays the placeholders and their replacements in files without modifying them.

## SYNTAX

```
Show-ReplaceContent [-Directory <String>] [-File <String>] [-ReplacementsDirectory <String>]
 [-FileExtensions <String[]>]
 [-FileType <System.Collections.Generic.Dictionary`2[System.String,Brimborium.ReplaceContent.RCFileType]>]
 [-Replacements <System.Collections.Generic.Dictionary`2[System.String,System.String]>] [-PassThou]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The `Show-ReplaceContent` cmdlet scans files for placeholder tags and displays what would be replaced without actually modifying the files. This is useful for previewing changes before applying them with `Update-ReplaceContent`.

The cmdlet identifies placeholders within comment blocks using the syntax `<Placeholder Name>` and `</Placeholder>`, and shows how they would be replaced with the specified replacement text.

## EXAMPLES

### Example 1: Show potential replacements in a single file
```powershell
Show-ReplaceContent -File ".\src\Program.cs" -ReplacementsDirectory ".\Replacements"
```

This example shows what content would be replaced in Program.cs using replacements defined in the ".\Replacements" directory.

### Example 2: Show potential replacements in all C# files in a directory
```powershell
Show-ReplaceContent -Directory ".\src" -FileExtensions ".cs" -ReplacementsDirectory ".\Replacements"
```

This example scans all .cs files in the src directory and its subdirectories, and shows what content would be replaced between placeholder tags with the corresponding replacements from the ".\Replacements" directory.

### Example 3: Show potential replacements using inline replacements
```powershell
$replacements = @{
    "Copyright" = "Copyright (c) 2023 Contoso Ltd. All rights reserved."
    "Version" = "1.2.3"
}
Show-ReplaceContent -Directory ".\src" -Replacements $replacements
```

This example shows what content would be replaced in all files in the src directory using the replacements defined in the $replacements hashtable.

## PARAMETERS

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Directory
Specifies the directory to scan for files. The cmdlet will process all files in this directory and its subdirectories that match the specified file extensions.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -File
Specifies a single file to process. Use this parameter when you want to show potential replacements in just one file.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileExtensions
Specifies an array of file extensions to filter the files to be processed. Only files with these extensions will be scanned for placeholders.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileType
Specifies a dictionary of file extensions and their corresponding RCFileType objects. This allows customizing the comment syntax for different file types.

```yaml
Type: System.Collections.Generic.Dictionary`2[System.String,Brimborium.ReplaceContent.RCFileType]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThou
When specified, returns the content objects for further processing instead of displaying them.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Replacements
Specifies a dictionary of placeholder names and their replacement values. Use this parameter to provide replacements directly instead of using files in the ReplacementsDirectory.

```yaml
Type: System.Collections.Generic.Dictionary`2[System.String,System.String]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplacementsDirectory
Specifies the directory containing replacement files. Each .txt file in this directory will be used as a replacement, with the filename (without extension) as the placeholder name and the file content as the replacement value. JSON files can also be used to define multiple replacements.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
This cmdlet does not accept pipeline input.

## OUTPUTS

### System.Object
By default, this cmdlet returns formatted information about the placeholders and their potential replacements.

When used with the -PassThou parameter, it returns RCContent objects that can be further processed.

## NOTES
The placeholder syntax uses comment blocks specific to each file type. For example:
- C#, JavaScript, TypeScript: `/* <Placeholder Name> */` and `/* </Placeholder> */`
- PowerShell: `<# <Placeholder Name> #>` and `<# </Placeholder> #>`
- HTML: `<!-- <Placeholder Name> -->` and `<!-- </Placeholder> -->`

This cmdlet is useful for previewing changes before applying them with `Update-ReplaceContent`.

## RELATED LINKS

[Update-ReplaceContent](Update-ReplaceContent.md)
