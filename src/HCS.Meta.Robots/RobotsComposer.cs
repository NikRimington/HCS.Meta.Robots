using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace HCS.Meta.Robots;

internal class RobotsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ManifestFilters().Append<RobotsManifestFilter>();
        builder.AddRobotsMeta();

    }   
}
