# llms.txt File Path Support — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow llms.txt content to be loaded from a `.md` or `.json` file on disk, per-domain and globally, instead of requiring inline appsettings configuration.

**Architecture:** Add `FilePath` to `MetaLlmSiteConfiguration` and `DefaultFilePath` to `MetaLlmsOptions`. A new internal `LlmsFileContent` DTO handles JSON deserialization without the `required Domain` constraint. `RobotFilesController.Llms()` resolves paths against `IWebHostEnvironment.ContentRootPath`, reads files, and serves content based on extension (`.md` = raw, `.json` = rendered markdown). Errors fall through to inline config.

**Tech Stack:** C# 13, .NET 9/10, ASP.NET Core, System.Text.Json, Umbraco 16/17

---

## File Map

| Action | Path | Responsibility |
|--------|------|----------------|
| Modify | `src/HCS.Meta.Robots/Models/MetaLlmsOptions.cs` | Add `DefaultFilePath` property |
| Modify | `src/HCS.Meta.Robots/Models/MetaLlmSiteConfiguration.cs` | Add `FilePath` property |
| Create | `src/HCS.Meta.Robots/Models/LlmsFileContent.cs` | Internal DTO for JSON file deserialization |
| Modify | `src/HCS.Meta.Robots/RobotFilesController.cs` | File reading logic in `Llms()`, inject `IWebHostEnvironment` |
| Modify | `src/HCS.Meta.Robots/appsettings-schema.HCS.Meta.Robots.json` | Schema for new properties |
| Modify | `src/HCS.Meta.Robots.TestSite/appsettings-schema.HCS.Meta.Robots.json` | Keep in sync with main schema |
| Modify | `src/HCS.Meta.Robots.TestSite/appsettings.json` | Add `FilePath` example |
| Create | `src/HCS.Meta.Robots.TestSite/llms/localhost.md` | Example markdown file for TestSite |
| Create | `src/HCS.Meta.Robots.TestSite/llms/localhost.json` | Example JSON file for TestSite |

---

### Task 1: Add `DefaultFilePath` to `MetaLlmsOptions`

**Files:**
- Modify: `src/HCS.Meta.Robots/Models/MetaLlmsOptions.cs`

- [ ] **Step 1: Add property**

Replace the entire file content with:

```csharp
namespace HCS.Meta.Robots.Models;

public class MetaLlmsOptions
{
    public const string Key = "HCS:Meta:Llms";
    public bool LlmsEnabled { get; set; } = false;
    public string DefaultTitle { get; set; } = string.Empty;
    public string? DefaultFilePath { get; set; }
    public MetaLlmSiteConfiguration[] Configurations { get; set; } = [];
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build src/HCS.Meta.Robots/HCS.Meta.Robots.csproj
```

Expected: `Build succeeded.`

---

### Task 2: Add `FilePath` to `MetaLlmSiteConfiguration`

**Files:**
- Modify: `src/HCS.Meta.Robots/Models/MetaLlmSiteConfiguration.cs`

- [ ] **Step 1: Add property**

Add `FilePath` after the `Domain` property. The file should become:

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HCS.Meta.Robots.Models;

public class MetaLlmSiteConfiguration
{
    [Required]
    public required string Domain { get; set; }
    public string? FilePath { get; set; }
    [Required]
    public required string Name { get; set; }
    public string? Summary { get; set; }
    public string? AdditionalNotes { get; set; }
    public LlmsLinkSection[]? Sections { get; set; }

    public override string ToString()
    {
        var body = new StringBuilder();
        body.Append("## ")
            .AppendLine(Name)
            .AppendLine();

        if(!string.IsNullOrWhiteSpace(Summary))
        {
            body.Append("> ").AppendLine(Summary).AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(AdditionalNotes))
        {
            body.AppendLine(AdditionalNotes).AppendLine();
        }

        foreach (var section in Sections ?? [])
        {
            body.AppendLine(section.ToString()).AppendLine();
        }
        return body.ToString();
    }
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build src/HCS.Meta.Robots/HCS.Meta.Robots.csproj
```

Expected: `Build succeeded.`

---

### Task 3: Create `LlmsFileContent` DTO

**Files:**
- Create: `src/HCS.Meta.Robots/Models/LlmsFileContent.cs`

`MetaLlmSiteConfiguration.Domain` is a C# `required` property — deserializing JSON that omits it throws. This DTO avoids that by having all properties optional. `Domain` is excluded because domain matching has already happened before file loading.

- [ ] **Step 1: Create file**

```csharp
using System.Text;

namespace HCS.Meta.Robots.Models;

internal class LlmsFileContent
{
    public string? Name { get; set; }
    public string? Summary { get; set; }
    public string? AdditionalNotes { get; set; }
    public LlmsLinkSection[]? Sections { get; set; }

    public string ToMarkdown()
    {
        var body = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Name))
            body.Append("## ").AppendLine(Name).AppendLine();

        if (!string.IsNullOrWhiteSpace(Summary))
            body.Append("> ").AppendLine(Summary).AppendLine();

        if (!string.IsNullOrWhiteSpace(AdditionalNotes))
            body.AppendLine(AdditionalNotes).AppendLine();

        foreach (var section in Sections ?? [])
            body.AppendLine(section.ToString()).AppendLine();

        return body.ToString();
    }
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build src/HCS.Meta.Robots/HCS.Meta.Robots.csproj
```

Expected: `Build succeeded.`

---

### Task 4: Update `RobotFilesController` with file path logic

**Files:**
- Modify: `src/HCS.Meta.Robots/RobotFilesController.cs`

Changes: inject `IWebHostEnvironment`, store private `_logger` field, update `Llms()` decision tree, add private `ReadFileContent(string)` method.

- [ ] **Step 1: Replace file content**

```csharp
using System.Text;
using System.Text.Json;
using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Meta.Robots;

public class RobotFilesController : UmbracoPageController
{
    private const string DefaultRobots = "User-agent: *\nDisallow: /app_data\nDisallow: /app_plugins/\nDisallow: /install\nDisallow: /bin\nDisallow: /umbraco/";

    private readonly ILogger<RobotFilesController> _logger;
    private readonly MetaRobotOptionsModel _robotsConfig;
    private readonly MetaLlmsOptions _llmsConfig;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RobotFilesController(
        ILogger<RobotFilesController> logger,
        ICompositeViewEngine compositeViewEngine,
        IOptionsMonitor<MetaRobotOptionsModel> robotsOptions,
        IOptions<MetaLlmsOptions> llmsOptions,
        IWebHostEnvironment env)
        : base(logger, compositeViewEngine)
    {
        _logger = logger;
        _robotsConfig = robotsOptions.CurrentValue;
        _llmsConfig = llmsOptions.Value;
        _env = env;
    }

    public IActionResult Robots()
    {
        if (_robotsConfig.RobotsEnabled)
        {
            Response.Headers.Append("Cache-Control", "max-age=10");
            return Content(GetRobotsValue(), "text/plain", Encoding.UTF8);
        }

        return Content("User-agent: *\nDisallow: /", "text/plain", Encoding.UTF8);
    }

    public IActionResult Llms()
    {
        var host = Request.Host.Host;
        var siteConfig = _llmsConfig.Configurations
            .FirstOrDefault(c => c.Domain.Equals(host, StringComparison.OrdinalIgnoreCase));

        Response.Headers.Append("Cache-Control", "max-age=10");

        if (siteConfig != null)
        {
            if (!string.IsNullOrWhiteSpace(siteConfig.FilePath))
            {
                var fileContent = ReadFileContent(siteConfig.FilePath);
                if (fileContent != null)
                    return Content(fileContent, "text/plain", Encoding.UTF8);
            }

            var body = new StringBuilder();
            body.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
            body.Append(siteConfig.ToString());
            return Content(body.ToString(), "text/plain", Encoding.UTF8);
        }

        if (!string.IsNullOrWhiteSpace(_llmsConfig.DefaultFilePath))
        {
            var fileContent = ReadFileContent(_llmsConfig.DefaultFilePath);
            if (fileContent != null)
                return Content(fileContent, "text/plain", Encoding.UTF8);
        }

        var fallback = new StringBuilder();
        fallback.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
        return Content(fallback.ToString(), "text/plain", Encoding.UTF8);
    }

    private string? ReadFileContent(string filePath)
    {
        var resolvedPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(_env.ContentRootPath, filePath);

        if (!File.Exists(resolvedPath))
        {
            _logger.LogWarning("llms.txt file not found: {Path}", resolvedPath);
            return null;
        }

        var ext = Path.GetExtension(resolvedPath).ToLowerInvariant();

        if (ext == ".md")
            return File.ReadAllText(resolvedPath);

        if (ext == ".json")
        {
            try
            {
                var json = File.ReadAllText(resolvedPath);
                var content = JsonSerializer.Deserialize<LlmsFileContent>(json, _jsonOptions);
                if (content == null)
                    return null;

                var body = new StringBuilder();
                body.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
                body.Append(content.ToMarkdown());
                return body.ToString();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize llms.txt JSON file: {Path}", resolvedPath);
                return null;
            }
        }

        _logger.LogWarning("Unsupported llms.txt file extension '{Extension}' for path: {Path}", ext, resolvedPath);
        return null;
    }

    private string GetRobotsValue()
    {
        if (_robotsConfig.RobotsEntries.Length > 0)
            return _robotsConfig.RobotsAddToDefault
                ? $"{DefaultRobots}\n\n{string.Join("\n", _robotsConfig.RobotsEntries)}"
                : string.Join("\n", _robotsConfig.RobotsEntries);

        return DefaultRobots;
    }
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build src/HCS.Meta.Robots/HCS.Meta.Robots.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```powershell
git add src/HCS.Meta.Robots/Models/MetaLlmsOptions.cs `
       src/HCS.Meta.Robots/Models/MetaLlmSiteConfiguration.cs `
       src/HCS.Meta.Robots/Models/LlmsFileContent.cs `
       src/HCS.Meta.Robots/RobotFilesController.cs
git commit -m "[FEAT] Add file path support for llms.txt content"
```

---

### Task 5: Update JSON schema files

Both schema files are identical — update both in one task.

**Files:**
- Modify: `src/HCS.Meta.Robots/appsettings-schema.HCS.Meta.Robots.json`
- Modify: `src/HCS.Meta.Robots.TestSite/appsettings-schema.HCS.Meta.Robots.json`

- [ ] **Step 1: Update main project schema**

Replace `src/HCS.Meta.Robots/appsettings-schema.HCS.Meta.Robots.json` with:

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "properties": {
    "HCS": {
      "type": "object",
      "properties": {
        "Meta": {
          "type": "object",
          "properties": {
            "RobotsEnabled": {
              "type": "boolean",
              "description": "Enable robots.txt. When false, all agents are disallowed."
            },
            "RobotsAddToDefault": {
              "type": "boolean",
              "description": "Append RobotsEntries to the default rules instead of replacing them."
            },
            "RobotsEntries": {
              "type": "array",
              "description": "Custom robots.txt directives.",
              "items": {
                "type": "string"
              }
            },
            "Llms": {
              "type": "object",
              "description": "Configuration for llms.txt generation.",
              "properties": {
                "LlmsEnabled": {
                  "type": "boolean",
                  "description": "Register the /llms.txt route and serve content. When false, the route is not registered."
                },
                "DefaultTitle": {
                  "type": "string",
                  "description": "H1 title rendered at the top of llms.txt."
                },
                "DefaultFilePath": {
                  "type": "string",
                  "description": "Path to a .md or .json file to serve when no domain match is found. Relative paths resolve from ContentRootPath."
                },
                "Configurations": {
                  "type": "array",
                  "description": "Per-domain site configurations matched by request host.",
                  "items": {
                    "type": "object",
                    "required": [ "Domain" ],
                    "properties": {
                      "Domain": {
                        "type": "string",
                        "description": "Hostname to match against the request (e.g. example.com)."
                      },
                      "FilePath": {
                        "type": "string",
                        "description": "Path to a .md or .json file to serve for this domain. Relative paths resolve from ContentRootPath. When set, inline Name/Summary/Sections are ignored."
                      },
                      "Name": {
                        "type": "string",
                        "description": "Site name rendered as a section heading. Used when FilePath is not set."
                      },
                      "Summary": {
                        "type": "string",
                        "description": "Short description rendered as a blockquote."
                      },
                      "AdditionalNotes": {
                        "type": "string",
                        "description": "Free-form notes appended after the summary."
                      },
                      "Sections": {
                        "type": "array",
                        "description": "Link sections rendered as markdown list items.",
                        "items": {
                          "type": "object",
                          "required": [ "Title", "Url" ],
                          "properties": {
                            "Title": {
                              "type": "string",
                              "description": "Link display text."
                            },
                            "Url": {
                              "type": "string",
                              "description": "Link URL."
                            },
                            "Description": {
                              "type": "string",
                              "description": "Optional description appended after the link."
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
```

- [ ] **Step 2: Copy identical content to TestSite schema**

Replace `src/HCS.Meta.Robots.TestSite/appsettings-schema.HCS.Meta.Robots.json` with the exact same JSON content as above.

- [ ] **Step 3: Commit**

```powershell
git add src/HCS.Meta.Robots/appsettings-schema.HCS.Meta.Robots.json `
       src/HCS.Meta.Robots.TestSite/appsettings-schema.HCS.Meta.Robots.json
git commit -m "[AMEND] Update llms.txt schema with FilePath and DefaultFilePath"
```

Note: `required` on `Configurations` items is relaxed from `["Domain", "Name"]` to `["Domain"]` — `Name` is no longer required when `FilePath` is set.

---

### Task 6: Add TestSite example files and update appsettings

**Files:**
- Create: `src/HCS.Meta.Robots.TestSite/llms/localhost.md`
- Create: `src/HCS.Meta.Robots.TestSite/llms/localhost.json`
- Modify: `src/HCS.Meta.Robots.TestSite/appsettings.json`

- [ ] **Step 1: Create example markdown file**

Create `src/HCS.Meta.Robots.TestSite/llms/localhost.md`:

```markdown
# LLMS Test Site

> This content is loaded from a markdown file on disk.

This site is used for testing the HCS.Meta.Robots package.

## Documentation

- [Package Docs](/docs): Full documentation for HCS.Meta.Robots.

## Source

- [GitHub](https://github.com/NikRimington/HCS.Meta.Robots): Source code and issue tracker.
```

- [ ] **Step 2: Create example JSON file**

Create `src/HCS.Meta.Robots.TestSite/llms/localhost.json`:

```json
{
  "Name": "LLMS Test Site (JSON)",
  "Summary": "This content is loaded from a JSON file on disk.",
  "AdditionalNotes": "This site is used for testing the HCS.Meta.Robots package.",
  "Sections": [
    {
      "Title": "Package Docs",
      "Url": "/docs",
      "Description": "Full documentation for HCS.Meta.Robots."
    },
    {
      "Title": "GitHub",
      "Url": "https://github.com/NikRimington/HCS.Meta.Robots",
      "Description": "Source code and issue tracker."
    }
  ]
}
```

- [ ] **Step 3: Update appsettings.json to demonstrate FilePath**

In `src/HCS.Meta.Robots.TestSite/appsettings.json`, update the `Llms` section to add `DefaultFilePath` and a `FilePath` on the localhost config. The final `HCS` block should look like:

```json
"HCS": {
  "Meta": {
    "RobotsEnabled": true,
    "RobotsAddToDefault": true,
    "RobotsEntries": [
      "User-Agent: google\nDisallow: /umbraco-client/"
    ],
    "Llms": {
      "LlmsEnabled": true,
      "DefaultTitle": "LLMS Test Site",
      "DefaultFilePath": "llms/localhost.md",
      "Configurations": [
        {
          "Domain": "localhost",
          "FilePath": "llms/localhost.md",
          "Name": "LLMS Test Site",
          "Summary": "This is a test site for LLMS configuration.",
          "Sections": [
            {
              "Title": "Section One",
              "Url": "/llms/section-one",
              "Description": "An example section link."
            }
          ]
        }
      ]
    }
  }
}
```

Note: `Name`/`Summary`/`Sections` are left in place to demonstrate that they are ignored when `FilePath` is set, and serve as fallback if the file is removed.

- [ ] **Step 4: Build TestSite to verify**

```powershell
dotnet build src/HCS.Meta.Robots.TestSite/HCS.Meta.Robots.TestSite.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 5: Manual verification**

Run the TestSite:

```powershell
dotnet run --project src/HCS.Meta.Robots.TestSite/HCS.Meta.Robots.TestSite.csproj
```

Then in a browser or curl:

```powershell
# Should return raw markdown file content (no "# LLMS Test Site" prepended)
curl http://localhost:5000/llms.txt
```

Expected response starts with `# LLMS Test Site` (from the markdown file itself, not prepended by controller).

To test JSON path, temporarily change `FilePath` in appsettings.json to `llms/localhost.json` and re-run. Expected response starts with `# LLMS Test Site` (prepended by controller) followed by `## LLMS Test Site (JSON)`.

To test error fallback, change `FilePath` to `llms/nonexistent.md` and re-run. Expected: falls through to inline config (renders `Name`, `Summary`, `Sections`). Check app logs for `LogWarning` about file not found.

- [ ] **Step 6: Commit**

```powershell
git add src/HCS.Meta.Robots.TestSite/llms/localhost.md `
       src/HCS.Meta.Robots.TestSite/llms/localhost.json `
       src/HCS.Meta.Robots.TestSite/appsettings.json
git commit -m "[FEAT] Add TestSite example files for llms.txt file path feature"
```
