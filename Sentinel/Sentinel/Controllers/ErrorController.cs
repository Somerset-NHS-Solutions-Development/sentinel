using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentinel.Models;
using Sentinel.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace Sentinel.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly SentinelContext _db;

        public ErrorController(ILogger<ErrorController> logger, SentinelContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        [RequestSizeLimit(100_000)] // Limit request size to 100KB
        public async Task<IActionResult> PostError()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            string jsonStr = await reader.ReadToEndAsync();
            try
            {
                JavascriptError jError = JsonConvert.DeserializeObject<JavascriptError>(jsonStr);

                // Parse the user agent string
                var uaParser = Parser.GetDefault();
                ClientInfo ci = uaParser.Parse(jError.agent);

                // Check col/line number are numeric
                string message = jError.message.ToString();
                _ = int.TryParse(jError.lineno, out int lineNoAsInt);
                // This is for datatables which stuffs an error message in the column field!
                if (!int.TryParse(jError.colno, out int colNoAsInt))
                {
                    message = $"{jError.colno} {message}";
                }

                ErrorLog el = new ErrorLog
                {
                    Timestamp = DateTime.UtcNow,
                    ClientIp = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Application = jError.applicationName,
                    UserAgent = ci.UA.Family,
                    UserAgentVersion = ci.UA.Major,
                    Os = ci.OS.Family,
                    Osversion = ci.OS.Major,
                    Device = ci.Device.Family,
                    VueInfo = jError.vueinfo,
                    Message = message,
                    Source = jError.source.ToString(),
                    Line = lineNoAsInt,
                    Col = colNoAsInt,
                    StackTrace = jError.stack,
                    Url = jError.url,
                    Processed = false,
                    PageSource = jError.pageSource
                };

                _db.ErrorLogs.Add(el);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Can't deserialize error message: {jsonStr}");
                return BadRequest();
            }

            return Ok();
        }
    }
}
