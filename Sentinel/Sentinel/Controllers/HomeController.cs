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

namespace Sentinel.Controllers
{
    [Authorize(Roles = "User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SentinelContext _db;

        public HomeController(ILogger<HomeController> logger, SentinelContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var errors = await _db.ErrorLogs
                                .Where(w => !w.Processed)
                                .OrderByDescending(o => o.Timestamp)
                                .Take(1000) // TODO - take max 1000 for now... maybe return flag if >1k records
                                .ToListAsync();
            var applications = await _db.ErrorLogs.Select(s => s.Application).Distinct().ToListAsync();
            var agents = await _db.ErrorLogs.Select(s => s.UserAgent).Distinct().ToListAsync();
            var osList = await _db.ErrorLogs.Select(s => s.Os).Distinct().ToListAsync();
            var devices = await _db.ErrorLogs.Select(s => s.Device).Distinct().ToListAsync();

            HomePageViewModel vm = new HomePageViewModel
            { 
                errors = errors,
                applications = applications,
                agents = agents,
                osList = osList,
                devices = devices
            };

            return View(vm);
        }

        public async Task<IActionResult> GetData(string appName, string userAgent, string os, string device, DateTime? minDate = null, DateTime? maxDate = null)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError(exceptionHandlerPathFeature.Error, $"Uncaught error for path: {exceptionHandlerPathFeature.Path}");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
