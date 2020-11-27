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

namespace Sentinel.Controllers
{
    [Authorize] // TODO - add role
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SentinelContext _db;

        public HomeController(ILogger<HomeController> logger, SentinelContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var errors = _db.ErrorLogs
                            .Where(w => !w.Processed)
                            .OrderByDescending(o => o.Timestamp)
                            .Take(1000) // TODO - take max 1000 for now... maybe return flag if >1k records
                            .ToList();

            return View(errors);
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
