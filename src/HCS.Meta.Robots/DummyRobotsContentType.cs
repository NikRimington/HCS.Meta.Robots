using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models;

namespace HCS.Meta.Robots;

public class DummyRobotsContentType : IPublishedContentType
{
    public Guid Key => Guid.Empty;

    public int Id => -1;

    public string Alias => "HCS.Robots.Dummy";

    public PublishedItemType ItemType => PublishedItemType.Unknown;

    public HashSet<string> CompositionAliases => new();

    public ContentVariation Variations => ContentVariation.Nothing;

    public bool IsElement => false;

    public IEnumerable<IPublishedPropertyType> PropertyTypes => Array.Empty<IPublishedPropertyType>();

    public int GetPropertyIndex(string alias)
    {
        return 0;
    }

    public IPublishedPropertyType? GetPropertyType(string alias)
    {
        return null;
    }

    public IPublishedPropertyType? GetPropertyType(int index)
    {
        return null;
    }
}
