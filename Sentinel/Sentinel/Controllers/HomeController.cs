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
using SourcemapToolkit.SourcemapParser;
using System.IO;
using SourcemapToolkit.CallstackDeminifier;
using Sentinel.Util;
using System.Text;

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
            var unminified = false;
            var errorLogObj = await _db.ErrorLogs.FindAsync(id);
            var stackTrace = errorLogObj?.StackTrace ?? "";

            // Parse the stack trace to find out which file caused the error
            (string url, int line) = ExtractUrlAndLineNumber(stackTrace);
            // If it was a minified file see if we can un-minify the stack trace
            if (url.EndsWith(".min.js"))
            {
                try
                {
                    (string basePath, string filename) = GetBasePathAndFilename(url);
                    var httpClient = _clientFactory.CreateClient("WindowsAuth");
                    DeminifyStackTraceResult deminifyStackTraceResult;

                    // Check if source map in db
                    var sourceRecord = await GetSourceFromDatabase(errorLogObj.Application, filename);
                    if (sourceRecord != null)
                    {
                        ISourceMapProvider sourceMapProvider = new StringSourceMapProvider(sourceRecord.MapSource);
                        ISourceCodeProvider sourceCodeProvider = new HttpSourceCodeProvider(httpClient);
                        deminifyStackTraceResult = DeminifyStackTrace(stackTrace, sourceMapProvider, sourceCodeProvider);
                    }
                    else
                    {
                        ISourceMapProvider sourceMapProvider = new HttpSourceMapProvider(httpClient);
                        ISourceCodeProvider sourceCodeProvider = new HttpSourceCodeProvider(httpClient);
                        deminifyStackTraceResult = DeminifyStackTrace(stackTrace, sourceMapProvider, sourceCodeProvider);
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append(errorLogObj.Message).Append("\r\n");
                    foreach (var frameResult in deminifyStackTraceResult.DeminifiedStackFrameResults)
                    {
                        var frame = frameResult.DeminifiedStackFrame;
                        sb.Append($"at {frame.MethodName} ({basePath}/{frame.FilePath ?? "UNKNOWN"}:{frame.SourcePosition?.ZeroBasedLineNumber ?? 0}:{frame.SourcePosition?.ZeroBasedColumnNumber ?? 0})\r\n");
                    }
                    stackTrace = sb.ToString();
                    unminified = true;
                }
                catch (Exception ex)
                {
                    // Couldn't unminify - log warning and set stack trace back to original version
                    _logger.LogWarning($"Unable to unminify stack trace from {url}: {ex}");
                    stackTrace = errorLogObj?.StackTrace ?? "";
                }
            }

            return Json(new { stackTrace, unminified });
        }

        public async Task<IActionResult> ViewCode(int id)
        {
            ViewCodeModel vm = new ViewCodeModel
            {
                Lang = "markup",
                Code = null
            };
            var errorLogObj = await _db.ErrorLogs.FindAsync(id);
            var stackTrace = errorLogObj?.StackTrace;

            if (!String.IsNullOrEmpty(stackTrace))
            {
                // Parse the stack trace to find out which file caused the error
                (vm.Url, vm.Line) = ExtractUrlAndLineNumber(stackTrace);
                if (String.IsNullOrEmpty(vm.Url))
                {
                    // Probably didn't have a full stack trace
                    // Fallback to just fetching the file as specified in error log
                    vm.Url = errorLogObj.Url;
                    vm.Line = 1;
                }

                if (vm.Url != null)
                {
                    // Fetch the file
                    try
                    {
                        // Strip any query string
                        vm.Url = vm.Url.Split('?')[0];
                        if (vm.Url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                        {
                            vm.Lang = "js";
                        }

                        // Is this a minified .js file?
                        var isMinifiedJs = vm.Url.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase);
                        (string _, string filename) = GetBasePathAndFilename(vm.Url);
                        var httpClient = _clientFactory.CreateClient("WindowsAuth");
                        var sourceRecord = await GetSourceFromDatabase(errorLogObj.Application, filename);

                        if (isMinifiedJs && sourceRecord != null)
                        {
                            // Use the source map from the database, but the originial (minified) .js file from the server
                            ISourceMapProvider sourceMapProvider = new StringSourceMapProvider(sourceRecord.MapSource);
                            ISourceCodeProvider sourceCodeProvider = new HttpSourceCodeProvider(httpClient);
                            DeminifyStackTraceResult deminifyStackTraceResult = DeminifyStackTrace(stackTrace, sourceMapProvider, sourceCodeProvider);
                            var topFrame = GetTopValidFrame(deminifyStackTraceResult);
                            if (topFrame != null)
                            {
                                vm.Code = sourceRecord.FullSource;
                                vm.Line = topFrame.SourcePosition.ZeroBasedLineNumber + 1;
                                vm.Unminified = true;
                            }
                        }
                        else if (isMinifiedJs)
                        {
                            // See if source map and unminified source are available from site
                            ISourceMapProvider sourceMapProvider = new HttpSourceMapProvider(httpClient);
                            ISourceCodeProvider sourceCodeProvider = new HttpSourceCodeProvider(httpClient);
                            DeminifyStackTraceResult deminifyStackTraceResult = DeminifyStackTrace(stackTrace, sourceMapProvider, sourceCodeProvider);
                            var topFrame = GetTopValidFrame(deminifyStackTraceResult);
                            if (topFrame != null)
                            {
                                var unminifiedSourceFilename = topFrame.FilePath;
                                var lastSlash = vm.Url.LastIndexOf('/');
                                var unminifiedSourceUrl = vm.Url[0..lastSlash] + "/" + unminifiedSourceFilename;
                                // Now try and get the unminified file...
                                var response = await httpClient.GetAsync(unminifiedSourceUrl);
                                if (response.IsSuccessStatusCode)
                                {
                                    vm.Code = await response.Content.ReadAsStringAsync();
                                    vm.Line = topFrame.SourcePosition.ZeroBasedLineNumber + 1;
                                    vm.Unminified = true;
                                }
                            }
                        }
                        if (vm.Code == null)
                        {
                            // Either a minified js file that we can't unminify, or plain markup (HTML)
                            var response = await httpClient.GetAsync(vm.Url);
                            vm.Code = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "Unable to fetch code";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Unable to retrieve code from {vm.Url}: {ex.Message}");
                        vm.Code = "Unable to retrieve code";
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

        // Sometimes the top frame is invalid, with null 
        protected SourcemapToolkit.CallstackDeminifier.StackFrame GetTopValidFrame(DeminifyStackTraceResult stackTraceResult)
        {
            foreach (var frameResult in stackTraceResult.DeminifiedStackFrameResults)
            {
                var stackFrame = frameResult.DeminifiedStackFrame;

                if (stackFrame.FilePath != null && stackFrame.SourcePosition != null)
                {
                    return stackFrame;
                }
            }

            return null;
        }

        protected (string, int) ExtractUrlAndLineNumber(string stackTrace)
        {
            // Look for the first (url:line:column) pattern in the stack trace
            Regex rgx = new Regex(@"(?<=\()(.*):([0-9]*):[0-9]*\)");
            var match = rgx.Match(stackTrace);
            var url = match?.Groups?[1]?.Value;
            _ = Int32.TryParse(match?.Groups?[2]?.Value, out int line);

            return (url, line);
        }

        protected DeminifyStackTraceResult DeminifyStackTrace(string stackTrace, ISourceMapProvider sourceMapProvider, ISourceCodeProvider sourceCodeProvider)
        {
            StackTraceDeminifier sourceMapCallstackDeminifier = StackTraceDeminfierFactory.GetStackTraceDeminfier(sourceMapProvider, sourceCodeProvider);
            return sourceMapCallstackDeminifier.DeminifyStackTrace(stackTrace);
        }

        protected async Task<Source> GetSourceFromDatabase(string application, string filename)
        {
            return await _db.Sources.FirstOrDefaultAsync(f => f.Application == application && f.Filename == filename);
        }

        protected (string, string) GetBasePathAndFilename(string url)
        {
            var lastSlash = url.LastIndexOf('/');
            var basePath = url[0..lastSlash];
            var startOfFilename = lastSlash + 1;
            var filename = url[startOfFilename..];
            return (basePath, filename);
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
