using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using stranitza.Services;
using stranitza.Utility;
using Serilog;

namespace stranitza.Controllers.api
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GMailController : ControllerBase
    {
        private readonly StatisticService _stats;

        public GMailController(StatisticService stats)
        {
            _stats = stats;
        }

        public async Task<IActionResult> Alerts()
        {
            string requestInfo;
            Request.EnableBuffering();
            using (var requestMemoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(requestMemoryStream);

                var requestHeadersInfo = Request.Headers.Select(x => $"\t{x.Key} = {x.Value}").ToList();
                var requestCookiesInfo = Request.Cookies.Select(x => $"\t{x.Key} = {x.Value}").ToList();

                requestInfo = @$"

Google Alerts Http Request Information:

IP: {StranitzaExtensions.GetIp(HttpContext)}

Scheme: {Request.Scheme}
Host: {Request.Host}
Path: {Request.Path}
QueryString: {Request.QueryString}

Headers: {Request.Headers.Count}
{string.Join(Environment.NewLine, requestHeadersInfo)}

Cookies: {Request.Cookies.Count}
{string.Join(Environment.NewLine, requestCookiesInfo)}

Type: {Request.ContentType}
Body: {Request.ContentLength}
{StranitzaExtensions.ReadStreamInChunks(requestMemoryStream)}

";
                Log.Logger.Information(requestInfo);
            }

            return Ok(requestInfo);
        }

        public IActionResult Ping()
        {
            return Ok(new
            {
                ServerTime = DateTime.Now,
                Version = _stats.GetAppVersion()
            });
        }
    }
}
