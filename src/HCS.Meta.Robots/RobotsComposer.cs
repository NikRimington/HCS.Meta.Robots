using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

#if NET9_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;
#else
#endif

namespace HCS.Meta.Robots;

public sealed class RobotsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
#if NET9_0_OR_GREATER
        builder.Services.AddSingleton<IPackageManifestReader, RobotsManifestFilter>();
#else
        builder.ManifestFilters().Append<RobotsManifestFilter>();
#endif
        builder.AddRobotsMeta();

    }   
}
