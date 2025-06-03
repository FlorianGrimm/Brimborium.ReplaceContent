# Brimborium.ReplaceContent

Tool that replaces content between 'tags'.

## Purpose

Update the content of a file between two 'tags'.
The Syntax is within a MultiLine-Comment:
default:

```C
/* <Placeholder Name> */

/* </Placeholder Name> */
```

if the file is a PowerShell-Script (ends with .ps1):

```PowerShell
<# <Placeholder Name> #>

<# </Placeholder Name> #>
```

The indentation of the start tag is added to the content (per line).

## Example

Content of TableA-Columns.txt:

```Text
ColumnA,
ColumnB,
ColumnC
```

Content of Source.sql - Before:

```SQL
SELECT
    /* <Placeholder TableA-Columns> */
    OldContent
    /* </Placeholder TableA-Columns> */
FROM TableA
```

Content of Source.sql - After:

```SQL
SELECT
    /* Placeholder TableA-Columns */
    ColumnA,
    ColumnB,
    ColumnC
    /* /Placeholder TableA-Columns */
FROM TableA

```

## Usage - Command Line

Syntax:

```Text
ReplaceContent.exe [-File path] [-Directory path] [-ReplacementsDirectory path] [-Write] [-Verbose]
```

```cmd
ReplaceContent.exe -File Source.sql
```

```cmd
ReplaceContent.exe -Directory "src"
```

Parameters:

-File: the file to process.
-Directory: the directory with the files to process.
-ReplacementsDirectory: the directory with the replacement files.
-Write: write the changes to the file.
-Verbose: show more information.

## Usage - PowerShell

```PowerShell
Update-Content -File Source.sql -ReplaceDirectory "Replacements"
```

TODO: example with custom dictionary
