using System.ComponentModel.DataAnnotations;
using Umbraco.Extensions;

namespace HCS.Meta.Robots.Models;

public class LlmsLinkSection
{
    [Required]
    public required string Title { get; set; }
    [Required]
    public required string Url { get; set; }
    public string? Description { get; set; }

    public override string ToString()
    {
        return $"- [{Title}]({Url}){(Description.IsNullOrWhiteSpace() ? string.Empty : $": {Description}")}";
    }
}
