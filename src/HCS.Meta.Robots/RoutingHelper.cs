using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using System.Diagnostics.CodeAnalysis;
#if NET10_0
using Umbraco.Cms.Core.Services.Navigation;
#endif

namespace HCS.Meta.Robots;

public static class RoutingHelper
{
    public static IPublishedContent FindContentByDomain(ActionExecutingContext context)
    {
        var umbracoContext = GetUmbracoContext(context);
        var domain = DomainUtilities.SelectDomain(umbracoContext?.Domains?.GetAll(false), umbracoContext.CleanedUmbracoUrl);

#if NET10_0
        var documentNavigationQueryService = GetIDocumentNavigationQueryService(context);
        if (domain == null)
            return DefaultToFirstRoot(documentNavigationQueryService, umbracoContext);

        var content = umbracoContext.Content?.GetById(domain.ContentId);
        return content ?? DefaultToFirstRoot(documentNavigationQueryService, umbracoContext);
#else
        if (domain == null)
            return DefaultToFirstRoot(umbracoContext);

        var content = umbracoContext.Content?.GetById(domain.ContentId);
        return content ?? DefaultToFirstRoot(umbracoContext);
#endif
    }

#if NET10_0
    private static IPublishedContent DefaultToFirstRoot(IDocumentNavigationQueryService documentNavigationQueryService, IUmbracoContext umbracoContext)
    {
        var hasRootKeys = documentNavigationQueryService.TryGetRootKeys(out var rootKeys);
        if (hasRootKeys)
        {
            return umbracoContext.Content.GetById(rootKeys.FirstOrDefault()) ?? new DummyIPublishedContent();
        }

        return new DummyIPublishedContent();
    }
#else
    private static IPublishedContent DefaultToFirstRoot(IUmbracoContext umbracoContext)
        => umbracoContext.Content?.GetAtRoot().FirstOrDefault() ?? new DummyIPublishedContent();
#endif

    [return: NotNull]
    private static IUmbracoContext GetUmbracoContext(ActionContext actionContext)
    {
        var umbracoContext = actionContext.HttpContext.RequestServices.GetService<IUmbracoContextAccessor>()?.GetRequiredUmbracoContext();

        return umbracoContext ?? throw new InvalidOperationException("Umbraco Context is null");
    }

#if NET10_0
    [return: NotNull]
    private static IDocumentNavigationQueryService GetIDocumentNavigationQueryService(ActionContext actionContext)
    {
        var documentNavigationQueryService = actionContext.HttpContext.RequestServices.GetService<IDocumentNavigationQueryService>();

        return documentNavigationQueryService ?? throw new InvalidOperationException("IDocumentNavigationQueryService is null");
    }
#endif
}
