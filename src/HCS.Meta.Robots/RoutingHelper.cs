using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using System.Diagnostics.CodeAnalysis;
using HCS.Meta.Robots.Models;

namespace HCS.Meta.Robots;

public static class RoutingHelper
{
    public static IPublishedContent FindContentByDomain(ActionExecutingContext context)
    {
        var umbracoContext = GetUmbracoContext(context);
        var domain = DomainUtilities.SelectDomain(umbracoContext?.Domains?.GetAll(false), umbracoContext.CleanedUmbracoUrl);

        if (domain == null)
            return DefaultToFirstRoot(umbracoContext);

        var content = umbracoContext.Content?.GetById(domain.ContentId);
        return content ?? DefaultToFirstRoot(umbracoContext);
    }

    private static IPublishedContent DefaultToFirstRoot(IUmbracoContext umbracoContext)
        => umbracoContext.Content?.GetAtRoot().FirstOrDefault() ?? new DummyIPublishedContent();


    [return: NotNull]
    private static IUmbracoContext GetUmbracoContext(ActionContext actionContext)
    {
        var umbracoContext = actionContext.HttpContext.RequestServices.GetService<IUmbracoContextAccessor>()?.GetRequiredUmbracoContext();

        return umbracoContext ?? throw new InvalidOperationException("Umbraco Context is null");
    }
}
