using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace HCS.Meta.Robots.Composers
{
    internal class AddRobotsHeaderComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.Configure<UmbracoPipelineOptions>(options => {
                options.AddFilter(new UmbracoPipelineFilter(
                    "RobotsHeader",
                    _ => { },
                    applicationBuilder => {
                        applicationBuilder.UseMiddleware<AddRobotsHeaderMiddleware>();
                    },
                    _ => { }
                ));
            });
        }
    }
}
