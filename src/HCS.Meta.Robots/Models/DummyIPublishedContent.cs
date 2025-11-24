using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.ObjectModel;

namespace HCS.Meta.Robots.Models;

public class DummyIPublishedContent : IPublishedContent
{
    public int Id => -1;

    public string Name => "Robots Dummy Content";

    public string? UrlSegment => null;

    public int SortOrder => 0;

    public int Level => 0;

    public string Path => string.Empty;

    public int? TemplateId => null;

    public int CreatorId => -1;

    public DateTime CreateDate => DateTime.Now;

    public int WriterId => -1;

    public DateTime UpdateDate => CreateDate;

    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => new ReadOnlyDictionary<string, PublishedCultureInfo>(new Dictionary<string, PublishedCultureInfo>());

    public PublishedItemType ItemType => PublishedItemType.Unknown;

    public IPublishedContent? Parent => null;

    public IEnumerable<IPublishedContent> Children => [];

    public IEnumerable<IPublishedContent> ChildrenForAllCultures => [];

    public IPublishedContentType ContentType => new DummyRobotsContentType();

    public Guid Key => Guid.Empty;

    public IEnumerable<IPublishedProperty> Properties => [];

    public IPublishedProperty? GetProperty(string alias)
    {
        return null;
    }

    public bool IsDraft(string? culture = null)
    {
        return false;
    }

    public bool IsPublished(string? culture = null)
    {
        return true;
    }
}
