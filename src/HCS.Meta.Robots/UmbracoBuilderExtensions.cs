using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace HCS.Meta.Robots;

internal static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddRobotsMeta(this IUmbracoBuilder builder)
    {
        return builder.RegisterServices().RegisterOptions().RegisterRoutes();
    }

    private static IUmbracoBuilder RegisterServices(this IUmbracoBuilder builder)
    {
        builder.Services.Configure<UmbracoPipelineOptions>(options => {
            options.AddFilter(new UmbracoPipelineFilter(
                name: "RobotsHeader",
                preRouting: applicationBuilder => {
                    applicationBuilder.UseMiddleware<AddRobotsHeaderMiddleware>();
                }
            ));
        }).Configure<UmbracoRequestOptions>(options =>
            {
                var allowList = new[] { RoutePatterns.Default[0]};
                var next = options.HandleAsServerSideRequest;
                options.HandleAsServerSideRequest = httpRequest =>
                {
                    return allowList.Any(route => httpRequest.Path.Value?.EndsWith(route, StringComparison.InvariantCultureIgnoreCase) == true) || next(httpRequest);
                };
            });

        return builder;
    }


    private static IUmbracoBuilder RegisterOptions(this IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<MetaRobotOptionsModel>()
            .Bind(builder.Config.GetSection(MetaRobotOptionsModel.Key));

        return builder;
    }

    private static IUmbracoBuilder RegisterRoutes(this IUmbracoBuilder builder)
    {
        builder.Services.Configure<UmbracoPipelineOptions>(options => {
            options.AddFilter(new UmbracoPipelineFilter(
                name: "HCS.Meta.Robots",
                postRouting: applicationBuilder =>
                {
                    applicationBuilder.UseAuthentication();
                    applicationBuilder.UseAuthorization();
                    applicationBuilder.UseEndpoints(u =>
                    {
                        for (int i = 0; i < RoutePatterns.Default.Length; i++)
                        {
                            u.MapControllerRoute(
                                $"{nameof(RobotsTxtController)}_{i}",
                                RoutePatterns.Default[i],
                                new
                                {
                                    Controller = ControllerExtensions.GetControllerName<RobotsTxtController>(),
                                    Action = nameof(RobotsTxtController.Index)
                                })
                            .ForUmbracoPage(RoutingHelper.FindContentByDomain);
                        }
                    });
                }
            ));
        });

        return builder;
    }
}
