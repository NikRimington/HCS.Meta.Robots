# HCS.Meta.Robots

[![Downloads](https://img.shields.io/nuget/dt/HCS.Meta.Robots?color=cc9900)](https://www.nuget.org/packages/HCS.Meta.Robots/)
[![NuGet](https://img.shields.io/nuget/vpre/HCS.Meta.Robots?color=0273B3)](https://www.nuget.org/packages/HCS.Meta.Robots)
[![GitHub license](https://img.shields.io/github/license/NikRimington/HCS.Meta.Robots?color=8AB803)](../LICENSE)

Umbraco package that serves `robots.txt` and `llms.txt` from configuration. Robots defaults to deny-all unless explicitly enabled — ideal for multi-environment setups. LLM support serves structured content to AI crawlers at `/llms.txt`.

## Installation

```sh
dotnet add package HCS.Meta.Robots
```

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

When `RobotsEnabled` is `false` (default), all agents are disallowed:

```
User-agent: *
Disallow: /
```

When enabled with an empty `RobotsEntries`, the default response is:

```
User-agent: *
Disallow: /app_data
Disallow: /app_plugins/
Disallow: /install
Disallow: /bin
Disallow: /umbraco/
```

### robots.txt options

| Property | Type | Description |
|---|---|---|
| `RobotsEnabled` | bool | Enable `robots.txt`. Defaults to deny-all when false. |
| `RobotsEntries` | string[] | Custom directives. Replaces defaults unless `RobotsAddToDefault` is true. |
| `RobotsAddToDefault` | bool | Append `RobotsEntries` after the default rules instead of replacing them. |

**Routes:** `/robots.txt`, `/{local}/robots.txt`

---

## llms.txt

Serves structured content to LLM crawlers following the [llms.txt](https://llmstxt.org/) convention.

**Routes:** `/llms.txt`, `/{local}/llms.txt`

Add to `appsettings.json`:

```json
"HCS": {
  "Meta": {
    "Llms": {
      "LlmsEnabled": true,
      "DefaultTitle": "My Site",
      "DefaultFilePath": "llms/default.md",
      "Configurations": [
        {
          "Domain": "example.com",
          "FilePath": "llms/example.md"
        }
      ]
    }
  }
}
```

### Content resolution order

For each request, the package resolves content in this order:

1. Domain match found in `Configurations` **and** `FilePath` set → serve that file
2. Domain match found, no `FilePath` → render inline `Name`/`Summary`/`AdditionalNotes`/`Sections`
3. No domain match, `DefaultFilePath` set → serve that file
4. Fallback → render `DefaultTitle` heading only

### File formats

Both `.md` and `.json` files are supported.

**Markdown (`.md`):** served as-is.

**JSON (`.json`):** deserialised into a structured model and rendered as markdown. Example:

```json
{
  "name": "My Site",
  "summary": "A short description.",
  "additionalNotes": "Extra notes here.",
  "sections": [
    {
      "title": "Docs",
      "url": "/docs",
      "description": "Documentation pages."
    }
  ]
}
```

Relative file paths resolve from `ContentRootPath`.

### llms.txt options

**Root (`HCS.Meta.Llms`):**

| Property | Type | Description |
|---|---|---|
| `LlmsEnabled` | bool | Register the `/llms.txt` route. Route is not registered when false. |
| `DefaultTitle` | string | H1 title at top of output. |
| `DefaultFilePath` | string | `.md` or `.json` file to serve when no domain matches. |
| `Configurations` | array | Per-domain configurations. |

**Per-domain (`Configurations` items):**

| Property | Type | Required | Description |
|---|---|---|---|
| `Domain` | string | Yes | Hostname to match (e.g. `example.com`). |
| `FilePath` | string | No | `.md` or `.json` file for this domain. When set, inline fields are ignored. |
| `Name` | string | No | Site name rendered as a heading. |
| `Summary` | string | No | Short description rendered as a blockquote. |
| `AdditionalNotes` | string | No | Free-form notes appended after the summary. |
| `Sections` | array | No | Link sections rendered as a markdown list. |

**Section items (`Sections`):**

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | string | Yes | Link display text. |
| `Url` | string | Yes | Link URL. |
| `Description` | string | No | Optional description after the link. |

---

## Contributing

Contributions welcome! Please read the [Contributing Guidelines](CONTRIBUTING.md).

## Acknowledgments

[Lottie Pitcher](https://github.com/LottePitcher) for the [Opinionated Starter kit](https://github.com/LottePitcher/opinionated-package-starter).
