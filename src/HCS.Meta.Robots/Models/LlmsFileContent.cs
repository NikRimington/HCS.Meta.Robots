using System.Text;

namespace HCS.Meta.Robots.Models;

internal class LlmsFileContent
{
    public string? Name { get; set; }
    public string? Summary { get; set; }
    public string? AdditionalNotes { get; set; }
    public LlmsLinkSection[]? Sections { get; set; }

    public string ToMarkdown()
    {
        var body = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Name))
            body.Append("## ").AppendLine(Name).AppendLine();

        if (!string.IsNullOrWhiteSpace(Summary))
            body.Append("> ").AppendLine(Summary).AppendLine();

        if (!string.IsNullOrWhiteSpace(AdditionalNotes))
            body.AppendLine(AdditionalNotes).AppendLine();

        foreach (var section in Sections ?? [])
            body.AppendLine(section.ToString()).AppendLine();

        return body.ToString();
    }
}
