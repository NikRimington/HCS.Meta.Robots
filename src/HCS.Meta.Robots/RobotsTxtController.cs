using System.Text;
using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Meta.Robots;

public class RobotsTxtController : UmbracoPageController
{
    private const string DefaultRobots = "User-agent: *\nDisallow: /app_data\nDisallow: /app_plugins/\nDisallow: /install\nDisallow: /bin\nDisallow: /umbraco/";

    private readonly MetaRobotOptionsModel _config;
    public RobotsTxtController(ILogger<RobotsTxtController> logger, ICompositeViewEngine compositeViewEngine, IOptionsMonitor<MetaRobotOptionsModel> options)
        : base(logger, compositeViewEngine)
    {
        _config = options.CurrentValue;
    }

    public IActionResult Index()
    {
        var defaultContent = "User-agent: *\nDisallow: /";

        if (_config.RobotsEnabled)
        {
            Response.Headers.Append("Cache-Control", "max-age=10");
            return Content(GetRobotsValue(), "text/plain", Encoding.UTF8);
        }

        return Content(defaultContent, "text/plain", Encoding.UTF8);
    }

    private string GetRobotsValue()
    {
        if (_config.RobotsEntries.Length > 0)
            return _config.RobotsAddToDefault ? $"{DefaultRobots}\n\n{string.Join("\n", _config.RobotsEntries)}" :  string.Join("\n", _config.RobotsEntries);

        return DefaultRobots;
    }
}
