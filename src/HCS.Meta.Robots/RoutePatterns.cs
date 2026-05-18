namespace HCS.Meta.Robots;

public static class RoutePatterns
{
#pragma warning disable IDE1006 // Naming Styles
    public static string[] Default = ["/robots.txt", "/{local}/robots.txt"];
    public static string[] Llms = ["/llms.txt", "/{local}/llms.txt"];
#pragma warning restore IDE1006 // Naming Styles
}
