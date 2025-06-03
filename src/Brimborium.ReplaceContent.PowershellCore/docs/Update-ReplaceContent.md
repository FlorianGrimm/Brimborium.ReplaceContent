---
external help file: Brimborium.ReplaceContent.PowershellCore.dll-Help.xml
Module Name: Brimborium.ReplaceContent
online version:
schema: 2.0.0
---

# Update-ReplaceContent

## SYNOPSIS
Updates content in files by replacing text between placeholder tags.

## SYNTAX

```
Update-ReplaceContent [-Directory <String>] [-File <String>] [-ReplacementsDirectory <String>]
 [-FileExtensions <String[]>]
 [-FileType <System.Collections.Generic.Dictionary`2[System.String,Brimborium.ReplaceContent.RCFileType]>]
 [-Replacements <System.Collections.Generic.Dictionary`2[System.String,System.String]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The `Update-ReplaceContent` cmdlet scans files for placeholder tags and replaces the content between them with specified replacement text. Placeholders are defined within comment blocks using the syntax `<Placeholder Name>` and `</Placeholder>`.

This cmdlet is useful for maintaining consistent content across multiple files, such as copyright notices, license information, or code snippets that need to be synchronized.

## EXAMPLES

### Example 1: Update content in a single file
```powershell
Update-ReplaceContent -File ".\src\Program.cs" -ReplacementsDirectory ".\Replacements"
```

This example updates the content in Program.cs using replacements defined in the ".\Replacements" directory.

### Example 2: Update content in all C# files in a directory
```powershell
Update-ReplaceContent -Directory ".\src" -FileExtensions ".cs" -ReplacementsDirectory ".\Replacements"
```

This example scans all .cs files in the src directory and its subdirectories, replacing content between placeholder tags with the corresponding replacements from the ".\Replacements" directory.

### Example 3: Update content using inline replacements
```powershell
$replacements = @{
    "Copyright" = "Copyright (c) 2023 Contoso Ltd. All rights reserved."
    "Version" = "1.2.3"
}
Update-ReplaceContent -Directory ".\src" -Replacements $replacements
```

This example updates content in all files in the src directory using the replacements defined in the $replacements hashtable.

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
Specifies a single file to process. Use this parameter when you want to update content in just one file.

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
This cmdlet returns information about the files that were updated.

## NOTES
The placeholder syntax uses comment blocks specific to each file type. For example:
- C#, JavaScript, TypeScript: `/* <Placeholder Name> */` and `/* </Placeholder> */`
- PowerShell: `<# <Placeholder Name> #>` and `<# </Placeholder> #>`
- HTML: `<!-- <Placeholder Name> -->` and `<!-- </Placeholder> -->`

The cmdlet preserves indentation when replacing content.

## RELATED LINKS

[Show-ReplaceContent](Show-ReplaceContent.md)
