using Umbraco.Cms.Core.Manifest;

namespace HCS.Meta.Robots;

internal class RobotsManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        var assembly = typeof(RobotsManifestFilter).Assembly;

        manifests.Add(new PackageManifest
        {
            PackageName = "HCS.Meta.Robots",
            Version = assembly.GetName()?.Version?.ToString(3) ?? "0.1.0",
            AllowPackageTelemetry = true,
            Scripts = new string[] {
                // List any Script files
                // Urls should start '/App_Plugins/HCS.Meta.Robots/' not '/wwwroot/HCS.Meta.Robots/', e.g.
                // "/App_Plugins/HCS.Meta.Robots/Scripts/scripts.js"
            },
            Stylesheets = new string[]
            {
                // List any Stylesheet files
                // Urls should start '/App_Plugins/HCS.Meta.Robots/' not '/wwwroot/HCS.Meta.Robots/', e.g.
                // "/App_Plugins/HCS.Meta.Robots/Styles/styles.css"
            }
        });
    }
}
