using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
        var llmsEnabled = builder.Config.GetSection(MetaLlmsOptions.Key).GetValue<bool>("LlmsEnabled");

        builder.Services.Configure<UmbracoPipelineOptions>(options => {
            options.AddFilter(new UmbracoPipelineFilter(
                name: "RobotsHeader",
                postRouting: applicationBuilder => {
                    applicationBuilder.UseMiddleware<AddRobotsHeaderMiddleware>();
                }
            ));
        }).Configure<UmbracoRequestOptions>(options =>
            {
                var allowList = llmsEnabled
                    ? new[] { RoutePatterns.Default[0], RoutePatterns.Llms[0] }
                    : new[] { RoutePatterns.Default[0] };
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

        builder.Services.AddOptions<MetaLlmsOptions>()
            .Bind(builder.Config.GetSection(MetaLlmsOptions.Key));

        return builder;
    }

    private static IUmbracoBuilder RegisterRoutes(this IUmbracoBuilder builder)
    {
        var llmsEnabled = builder.Config.GetSection(MetaLlmsOptions.Key).GetValue<bool>("LlmsEnabled");

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
                                $"{nameof(RobotFilesController)}_Robots_{i}",
                                RoutePatterns.Default[i],
                                new
                                {
                                    Controller = ControllerExtensions.GetControllerName<RobotFilesController>(),
                                    Action = nameof(RobotFilesController.Robots)
                                })
                            .ForUmbracoPage(RoutingHelper.FindContentByDomain);
                        }

                        if (llmsEnabled)
                        {
                            for (int i = 0; i < RoutePatterns.Llms.Length; i++)
                            {
                                u.MapControllerRoute(
                                    $"{nameof(RobotFilesController)}_Llms_{i}",
                                    RoutePatterns.Llms[i],
                                    new
                                    {
                                        Controller = ControllerExtensions.GetControllerName<RobotFilesController>(),
                                        Action = nameof(RobotFilesController.Llms)
                                    })
                                .ForUmbracoPage(RoutingHelper.FindContentByDomain);
                            }
                        }
                    });
                }
            ));
        });

        return builder;
    }
}
