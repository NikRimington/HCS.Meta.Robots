namespace HCS.Meta.Robots.Models;

public class MetaRobotOptionsModel
{
    public const string Key = "HCS:Meta";

    public bool RobotsEnabled { get; set; } = false;

    public string[] RobotsEntries { get; set; } = Array.Empty<string>();
    public bool RobotsAddToDefault { get; set; } = false;
}
