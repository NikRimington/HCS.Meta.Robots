using System.Text;
using System.Text.Json;
using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Meta.Robots;

public class RobotFilesController : UmbracoPageController
{
    private const string DefaultRobots = "User-agent: *\nDisallow: /app_data\nDisallow: /app_plugins/\nDisallow: /install\nDisallow: /bin\nDisallow: /umbraco/";

    private readonly ILogger<RobotFilesController> _logger;
    private readonly MetaRobotOptionsModel _robotsConfig;
    private readonly MetaLlmsOptions _llmsConfig;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RobotFilesController(
        ILogger<RobotFilesController> logger,
        ICompositeViewEngine compositeViewEngine,
        IOptionsMonitor<MetaRobotOptionsModel> robotsOptions,
        IOptions<MetaLlmsOptions> llmsOptions,
        IWebHostEnvironment env)
        : base(logger, compositeViewEngine)
    {
        _logger = logger;
        _robotsConfig = robotsOptions.CurrentValue;
        _llmsConfig = llmsOptions.Value;
        _env = env;
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

        Response.Headers.Append("Cache-Control", "max-age=10");

        if (siteConfig != null)
        {
            if (!string.IsNullOrWhiteSpace(siteConfig.FilePath))
            {
                var fileContent = ReadFileContent(siteConfig.FilePath);
                if (fileContent != null)
                    return Content(fileContent, "text/plain", Encoding.UTF8);
            }

            var body = new StringBuilder();
            body.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
            body.Append(siteConfig.ToString());
            return Content(body.ToString(), "text/plain", Encoding.UTF8);
        }

        if (!string.IsNullOrWhiteSpace(_llmsConfig.DefaultFilePath))
        {
            var fileContent = ReadFileContent(_llmsConfig.DefaultFilePath);
            if (fileContent != null)
                return Content(fileContent, "text/plain", Encoding.UTF8);
        }

        var fallback = new StringBuilder();
        fallback.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
        return Content(fallback.ToString(), "text/plain", Encoding.UTF8);
    }

    private string? ReadFileContent(string filePath)
    {
        var resolvedPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(_env.ContentRootPath, filePath);

        if (!System.IO.File.Exists(resolvedPath))
        {
            _logger.LogWarning("llms.txt file not found: {Path}", resolvedPath);
            return null;
        }

        var ext = Path.GetExtension(resolvedPath).ToLowerInvariant();

        if (ext == ".md")
        {
            try
            {
                return System.IO.File.ReadAllText(resolvedPath);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to read llms.txt markdown file: {Path}", resolvedPath);
                return null;
            }
        }

        if (ext == ".json")
        {
            try
            {
                var json = System.IO.File.ReadAllText(resolvedPath);
                var content = JsonSerializer.Deserialize<LlmsFileContent>(json, _jsonOptions);
                if (content == null)
                    return null;

                var body = new StringBuilder();
                body.AppendLine($"# {_llmsConfig.DefaultTitle}").AppendLine();
                body.Append(content.ToMarkdown());
                return body.ToString();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize llms.txt JSON file: {Path}", resolvedPath);
                return null;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to read llms.txt JSON file: {Path}", resolvedPath);
                return null;
            }
        }

        _logger.LogWarning("Unsupported llms.txt file extension '{Extension}' for path: {Path}", ext, resolvedPath);
        return null;
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
