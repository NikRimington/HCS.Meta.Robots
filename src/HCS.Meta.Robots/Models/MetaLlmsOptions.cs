namespace HCS.Meta.Robots.Models;

public class MetaLlmsOptions
{
    public const string Key = "HCS:Meta:Llms";
    public bool LlmsEnabled { get; set; } = false;
    public string DefaultTitle { get; set; } = string.Empty;
    public MetaLlmSiteConfiguration[] Configurations { get; set; } = [];
}
