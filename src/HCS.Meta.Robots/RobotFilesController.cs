using System.Text;
using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Meta.Robots;

public class RobotFilesController : UmbracoPageController
{
    private const string DefaultRobots = "User-agent: *\nDisallow: /app_data\nDisallow: /app_plugins/\nDisallow: /install\nDisallow: /bin\nDisallow: /umbraco/";

    private readonly MetaRobotOptionsModel _robotsConfig;
    private readonly MetaLlmsOptions _llmsConfig;

    public RobotFilesController(ILogger<RobotFilesController> logger, ICompositeViewEngine compositeViewEngine,
        IOptionsMonitor<MetaRobotOptionsModel> robotsOptions, IOptions<MetaLlmsOptions> llmsOptions)
        : base(logger, compositeViewEngine)
    {
        _robotsConfig = robotsOptions.CurrentValue;
        _llmsConfig = llmsOptions.Value;
    }

    public IActionResult Robots()
    {
        if (_robotsConfig.RobotsEnabled)
        {
            Response.Headers.Append("Cache-Control", "max-age=10");
            return Content(GetRobotsValue(), "text/plain", Encoding.UTF8);
        }

        return Content("User-agent: *\nDisallow: /", "text/plain", Encoding.UTF8);
    }

    public IActionResult Llms()
    {
        var host = Request.Host.Host;
        var siteConfig = _llmsConfig.Configurations
            .FirstOrDefault(c => c.Domain.Equals(host, StringComparison.OrdinalIgnoreCase));

        var body = new StringBuilder();
        body.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();

        if (siteConfig != null)
            body.Append(siteConfig.ToString());

        Response.Headers.Append("Cache-Control", "max-age=10");
        return Content(body.ToString(), "text/plain", Encoding.UTF8);
    }

    private string GetRobotsValue()
    {
        if (_robotsConfig.RobotsEntries.Length > 0)
            return _robotsConfig.RobotsAddToDefault
                ? $"{DefaultRobots}\n\n{string.Join("\n", _robotsConfig.RobotsEntries)}"
                : string.Join("\n", _robotsConfig.RobotsEntries);

        return DefaultRobots;
    }
}
