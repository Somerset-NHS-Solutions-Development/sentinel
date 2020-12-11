using Sentinel.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sentinel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace Sentinel.Controllers
{
    [Authorize(Roles = "User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SentinelContext _db;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(ILogger<HomeController> logger, SentinelContext db, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _db = db;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // This page won't work in IE! Check user agent...
            string userAgent = Request.Headers["User-Agent"];
            if (IsInternetExplorer(userAgent))
            {
                return View("NotSupported");
            }

            var numRecords = await _db.ErrorLogs
                                .Where(w => !w.Processed)
                                .CountAsync();

            var errors = await _db.ErrorLogs
                                .Where(w => !w.Processed)
                                .OrderByDescending(o => o.Timestamp)
                                .Take(1000) // Take max 1000 for now... and flag if more available
                                .ToListAsync();

            var applications = await _db.ErrorLogs.Select(s => s.Application).Distinct().ToListAsync();
            var agents = await _db.ErrorLogs.Select(s => s.UserAgent).Distinct().ToListAsync();
            var osList = await _db.ErrorLogs.Select(s => s.Os).Distinct().ToListAsync();
            var devices = await _db.ErrorLogs.Select(s => s.Device).Distinct().ToListAsync();

            HomePageViewModel vm = new HomePageViewModel
            {
                Errors = errors,
                Applications = applications,
                Agents = agents,
                OsList = osList,
                Devices = devices,
                More = numRecords > 1000
            };

            return View(vm);
        }

        public async Task<IActionResult> GetData(string appName, string userAgent, string os, string device, string searchText, DateTime? minDate = null, DateTime? maxDate = null)
        {
            var errors = _db.ErrorLogs
                            .Where(w => !w.Processed);

            if (!String.IsNullOrEmpty(appName))
            {
                errors = errors.Where(w => w.Application == appName);
            }
            if (!String.IsNullOrEmpty(userAgent))
            {
                errors = errors.Where(w => w.UserAgent == userAgent);
            }
            if (!String.IsNullOrEmpty(os))
            {
                errors = errors.Where(w => w.Os == os);
            }
            if (!String.IsNullOrEmpty(device))
            {
                errors = errors.Where(w => w.Device == device);
            }
            if (!String.IsNullOrEmpty(searchText))
            {
                errors = errors.Where(w => w.Message.Contains(searchText));
            }
            if (minDate.HasValue)
            {
                errors = errors.Where(w => w.Timestamp > minDate.Value);
            }
            if (maxDate.HasValue)
            {
                // Include this day
                maxDate = maxDate.Value.AddDays(1);
                errors = errors.Where(w => w.Timestamp < maxDate.Value);
            }

            var sortedErrors = await errors.OrderByDescending(o => o.Timestamp).ToListAsync();

            return PartialView("_partialErrorData", sortedErrors);
        }

        public async Task<IActionResult> GetStackTrace(int id)
        {
            var errorLogObj = await _db.ErrorLogs.FindAsync(id);
            var stackTrace = errorLogObj?.StackTrace ?? "";
            return Json(new { stackTrace });
        }

        public async Task<IActionResult> ViewCode(int id)
        {
            ViewCodeModel vm = new ViewCodeModel();
            vm.lang = "markup";
            vm.code = "Unable to fetch code";
            var errorLogObj = await _db.ErrorLogs.FindAsync(id);
            var stackTrace = errorLogObj?.StackTrace;

            if (!String.IsNullOrEmpty(stackTrace))
            {
                // Parse the stack trace to find out which file caused the error
                // **** REFACTOR - TODO
                Regex rgx = new Regex(@"(?<=\()(.*):([0-9]*):[0-9]*\)");
                var match = rgx.Match(stackTrace);
                var url = match?.Groups?[1]?.Value;
                Int32.TryParse(match?.Groups?[2]?.Value, out int line);
                vm.line = line;

                if (url != null)
                {
                    // Fetch the file
                    var httpClient = _clientFactory.CreateClient();
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        vm.code = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "Unable to fetch code";
                        // Check if it's JavaScript
                        vm.url = url.Split('?')[0];
                        if (vm.url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                        {
                            vm.lang = "js";
                        }
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning($"Unable to retrieve code from {url}");
                    }
                }
            }

            return View(vm);
        }

        public async Task<IActionResult> GetSource(int id)
        {
            var errorLogObj = await _db.ErrorLogs.FindAsync(id);
            var source = errorLogObj?.Source ?? "";
            return Json(new { source });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsProcessed(List<int> ids)
        {
            try
            {
                var toUpdate = _db.ErrorLogs.Where(w => ids.Contains(w.Id));
                foreach (var el in toUpdate)
                {
                    el.Processed = true;
                }
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking as processed: {String.Join(',', ids)}");
                return BadRequest();
            }

            return Ok();
        }

        protected static bool IsInternetExplorer(string userAgent)
        {
            return userAgent.Contains("MSIE") || userAgent.Contains("Trident");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError(exceptionHandlerPathFeature.Error, $"Uncaught error for path: {exceptionHandlerPathFeature.Path}");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
