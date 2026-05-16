# llms.txt File Path Support — Design Spec

**Date:** 2026-05-16
**Branch:** dev/v2

## Summary

Add support for pointing llms.txt content to a file on disk, rather than requiring all content to be defined inline in appsettings. Supports per-domain file paths and a global default file path. Files can be Markdown (served as raw response) or JSON (deserialized as `MetaLlmSiteConfiguration` and rendered via existing logic).

## Model Changes

### `MetaLlmSiteConfiguration` (per-domain)

Add `FilePath` (string?) property. When set, content is loaded from disk instead of using inline config properties (`Name`, `Summary`, `AdditionalNotes`, `Sections`).

### `MetaLlmsOptions` (global)

Add `DefaultFilePath` (string?) property. Used when no domain match is found, as a fallback file source.

### Path Resolution

Paths may be absolute or relative. Relative paths are resolved against `IWebHostEnvironment.ContentRootPath`. This keeps file references portable across environments.

## Controller Logic

Changes confined to `RobotFilesController.Llms()`. Decision tree:

1. **Domain match found + `FilePath` set** → read file from disk
   - `.md` extension → return raw file content as entire response (no title prepended, no wrapping)
   - `.json` extension → deserialize as `MetaLlmSiteConfiguration`, render via `ToString()`, wrap with `# {DefaultTitle}`
2. **Domain match found + no `FilePath`** → existing inline behavior (unchanged)
3. **No domain match + `DefaultFilePath` set** → read file from disk, same `.md`/`.json` logic as above
4. **No domain match + no `DefaultFilePath`** → existing behavior (`# {DefaultTitle}` or empty)

## File Formats

### Markdown (`.md`)

The file content is served verbatim as the entire HTTP response body. No modification, no title prepended. The file is assumed to be a complete, well-formed llms.txt document.

### JSON (`.json`)

Matches the shape of `MetaLlmSiteConfiguration` but `Domain` is omitted (domain matching already happened; including `Domain` in the file would be redundant and confusing). Because `MetaLlmSiteConfiguration.Domain` is a C# `required` property, deserializing directly into it would throw. A small internal DTO `LlmsFileContent` is introduced with the same properties minus `Domain`, all optional. Content is rendered the same way as inline config (via equivalent `ToString()` logic), wrapped with `# {DefaultTitle}`.

`LlmsFileContent` properties: `Name` (string?), `Summary` (string?), `AdditionalNotes` (string?), `Sections` (LlmsLinkSection[]?).

Example JSON file:
```json
{
  "Name": "My Site",
  "Summary": "Short description of the site.",
  "AdditionalNotes": "Extra free-form notes.",
  "Sections": [
    {
      "Title": "Docs",
      "Url": "/docs",
      "Description": "Full documentation."
    }
  ]
}
```

## Error Handling

- **File not found** → log warning, fall through to inline config (if set) or empty output
- **Invalid JSON** → log error, fall through to inline config (if set) or empty output
- **Unknown extension** → log warning, treat as unresolvable, fall through

## Configuration Example

```json
{
  "HCS": {
    "Meta": {
      "Llms": {
        "LlmsEnabled": true,
        "DefaultTitle": "My Platform",
        "DefaultFilePath": "llms/default.md",
        "Configurations": [
          {
            "Domain": "example.com",
            "FilePath": "llms/example-com.md"
          },
          {
            "Domain": "api.example.com",
            "FilePath": "llms/api-example-com.json"
          },
          {
            "Domain": "legacy.example.com",
            "Name": "Legacy Site",
            "Summary": "Inline config still works unchanged."
          }
        ]
      }
    }
  }
}
```

## Schema Changes

`appsettings-schema.HCS.Meta.Robots.json` needs two new string properties added:
- `HCS.Meta.Llms.DefaultFilePath`
- Each configuration item gains `FilePath`

## Files to Change

| File | Change |
|------|--------|
| `Models/MetaLlmsOptions.cs` | Add `DefaultFilePath` string? property |
| `Models/MetaLlmSiteConfiguration.cs` | Add `FilePath` string? property |
| `Models/LlmsFileContent.cs` | New internal DTO for JSON file deserialization |
| `RobotFilesController.cs` | Update `Llms()` to handle file path logic; inject `IWebHostEnvironment` |
| `appsettings-schema.HCS.Meta.Robots.json` (TestSite) | Add new properties to schema |
| `appsettings.json` (TestSite) | Add example `FilePath` usage |

## Out of Scope

- File watching / hot reload (YAGNI — endpoint has `Cache-Control: max-age=10`)
- Memory caching of file contents
- Support for other file extensions beyond `.md` and `.json`
- Per-domain `DefaultTitle` override (not requested)
