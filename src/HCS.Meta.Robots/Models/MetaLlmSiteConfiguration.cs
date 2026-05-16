using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HCS.Meta.Robots.Models;

public class  MetaLlmSiteConfiguration
{
    [Required]
    public required string Domain { get; set; }
    [Required]
    public required string Name { get; set; }
    public string? Summary { get; set; }

    public string? AdditionalNotes { get; set; }
    public LlmsLinkSection[]? Sections { get; set; }

    public override string ToString()
    {
        var body = new StringBuilder();
        body.Append("## ")
            .AppendLine(Name)
            .AppendLine();

        if(!string.IsNullOrWhiteSpace(Summary))
        {
            body.Append("> ").AppendLine(Summary).AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(AdditionalNotes))
        {
            body.AppendLine(AdditionalNotes).AppendLine();
        }

        foreach (var section in Sections ?? Array.Empty<LlmsLinkSection>())
        {
            body.AppendLine(section.ToString()).AppendLine();
        }
        return body.ToString();
    }
}
