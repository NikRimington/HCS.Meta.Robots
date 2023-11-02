# HCS.Meta.Robots

[![Downloads](https://img.shields.io/nuget/dt/HCS.Meta.Robots?color=cc9900)](https://www.nuget.org/packages/HCS.Meta.Robots/)
[![NuGet](https://img.shields.io/nuget/vpre/HCS.Meta.Robots?color=0273B3)](https://www.nuget.org/packages/HCS.Meta.Robots)
[![GitHub license](https://img.shields.io/github/license/NikRimington/HCS.Meta.Robots?color=8AB803)](../LICENSE)

Easily configurable Robots.txt defaulting to a deny all unless enabled. Perfect for when you have multiple environments.

<!--
Including screenshots is a really good idea! 

If you put images into /docs/screenshots, then you would reference them in this readme as, for example:

<img alt="..." src="https://github.com/NikRimington/HCS.Meta.Robots/blob/develop/docs/screenshots/screenshot.png">
-->

## Installation

Add the package to an existing Umbraco website (v10.4+) from nuget:

`dotnet add package HCS.Meta.Robots`

To enable robots.txt and prevent it from returning a deny all add the following to your Appsettings.json

```json
"HCS": {
    "Meta": {
        "RobotsEnabled" : true,
        "RobotsEntries" : []
    }
}
```

When enabled with an emtpy RobotsEntries array the response will be:

```txt
User-agent: *
Disallow: /app_data
Disallow: /app_plugins/
Disallow: /install
Disallow: /bin
Disallow: /umbraco/
```

## Additional options

`RobotsAddToDefault` if set to true, the entries in RobotsEntries will be merged with the default response rather than replacing it *note, they are merged AFTER the default*.

## Contributing

Contributions to this package are most welcome! Please read the [Contributing Guidelines](CONTRIBUTING.md).

## Acknowledgments

[Lottie Pitcher](https://github.com/LottePitcher) for the [Opinionated Starter kit](https://github.com/LottePitcher/opinionated-package-starter) for helping me get this package off the ground.