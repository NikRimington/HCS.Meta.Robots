# HCS.Meta.Robots

[![Downloads](https://img.shields.io/nuget/dt/HCS.Meta.Robots?color=cc9900)](https://www.nuget.org/packages/HCS.Meta.Robots/)
[![NuGet](https://img.shields.io/nuget/vpre/HCS.Meta.Robots?color=0273B3)](https://www.nuget.org/packages/HCS.Meta.Robots)
[![GitHub license](https://img.shields.io/github/license/NikRimington/HCS.Meta.Robots?color=8AB803)](https://github.com/NikRimington/HCS.Meta.Robots/blob/main/LICENSE)

Umbraco package that serves `robots.txt` and `llms.txt` from configuration. Robots defaults to deny-all unless explicitly enabled — ideal for multi-environment setups. LLM support serves structured content to AI crawlers at `/llms.txt`.

## Installation

```sh
dotnet add package HCS.Meta.Robots
```

Requires Umbraco 18+.

---

## robots.txt

Add to `appsettings.json`:

```json
"HCS": {
  "Meta": {
    "RobotsEnabled": true,
    "RobotsEntries": [],
    "RobotsAddToDefault": false
  }
}
```

When `RobotsEnabled` is `false` (default), all agents are disallowed. When enabled with an empty `RobotsEntries`, sensible Umbraco defaults are served.

Set `RobotsAddToDefault: true` to append your `RobotsEntries` after the defaults rather than replacing them.

**Routes:** `/robots.txt`, `/{local}/robots.txt`

---

## llms.txt

Serves structured content to LLM crawlers following the [llms.txt](https://llmstxt.org/) convention.

**Routes:** `/llms.txt`, `/{local}/llms.txt`

### Quick start — file-based

```json
"HCS": {
  "Meta": {
    "Llms": {
      "LlmsEnabled": true,
      "DefaultTitle": "My Site",
      "DefaultFilePath": "llms/default.md"
    }
  }
}
```

Place a markdown file at `<ContentRoot>/llms/default.md`. It is served as-is.

### Per-domain configuration

```json
"HCS": {
  "Meta": {
    "Llms": {
      "LlmsEnabled": true,
      "DefaultTitle": "My Site",
      "Configurations": [
        {
          "Domain": "example.com",
          "Name": "Example Site",
          "Summary": "A short description of this site.",
          "AdditionalNotes": "Extra context for LLMs.",
          "Sections": [
            {
              "Title": "Documentation",
              "Url": "/docs",
              "Description": "Full documentation."
            }
          ]
        }
      ]
    }
  }
}
```

Set `FilePath` on a configuration entry to serve a `.md` or `.json` file for that domain instead of using the inline fields.

### JSON file format

```json
{
  "name": "My Site",
  "summary": "A short description.",
  "additionalNotes": "Extra notes.",
  "sections": [
    { "title": "Docs", "url": "/docs", "description": "Documentation." }
  ]
}
```

Relative file paths resolve from `ContentRootPath`.

---

For full documentation and source, see the [GitHub repository](https://github.com/NikRimington/HCS.Meta.Robots).
