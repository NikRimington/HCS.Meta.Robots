using HCS.Meta.Robots.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace HCS.Meta.Robots
{
    public class AddRobotsHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRuntimeState _runtimeState;
        private readonly MetaRobotOptionsModel _config;

        public AddRobotsHeaderMiddleware(RequestDelegate next, IRuntimeState runtimeState, IOptions<MetaRobotOptionsModel> options)
        {
            _next = next;
            _runtimeState = runtimeState;
            _config = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // If Umbraco hasn't been installed yet, the middleware shouldn't do anything
            if (_runtimeState.Level == RuntimeLevel.Install)
            {
                await _next(context);
                return;
            }

            context.Response.OnStarting(() => {

                if (!_config.RobotsEnabled)
                {
                    context.Response.Headers.Add("X-Robots-Tag", "noindex");
                }

                return Task.CompletedTask;

            });

            await _next(context);
        }
    }
}
