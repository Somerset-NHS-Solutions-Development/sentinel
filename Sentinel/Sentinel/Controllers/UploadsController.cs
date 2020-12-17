using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sentinel.Models;
using Sentinel.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Controllers
{
    [Authorize(Roles = "User")]
    public class UploadsController : Controller
    {
        private readonly ILogger<UploadsController> _logger;
        private readonly SentinelContext _db;

        public UploadsController(ILogger<UploadsController> logger, SentinelContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _db.Sources.Select(s => new UploadSummaryViewModel(s)).ToListAsync();
            return View(vm);
        }

        public IActionResult Create()
        {
            var vm = new EditUploadViewModel();
            return View("CreateEdit", vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var source = await _db.Sources.FindAsync(id);
            if (source == null) return BadRequest();

            var vm = new EditUploadViewModel(source);
            return View("CreateEdit", vm);
        }

        [HttpPost]
        [RequestSizeLimit(500_000)] // Limit file uploads to 500KB total
        public async Task<IActionResult> CreateEdit(EditUploadViewModel model)
        {
            try
            {
                if (model.SourceMap != null && model.SourceMap.ContentType != "text/plain")
                {
                    TempData["error"] = "Invalid content type for source map";
                    return RedirectToAction(nameof(Index));
                }
                else if (model.FullSource != null && model.FullSource.ContentType != "text/javascript")
                {
                    TempData["error"] = "Invalid content type for unminified source";
                    return RedirectToAction(nameof(Index));
                }
                if (model.Id == 0)
                {
                    // Create
                    if (model.SourceMap == null || model.FullSource == null)
                    {
                        TempData["error"] = "Source map and unminified source must be supplied!";
                        return RedirectToAction(nameof(Index));
                    }
                    Source s = new Source();
                    await model.UpdateSource(s);
                    _db.Sources.Add(s);
                    await _db.SaveChangesAsync();
                    TempData["message"] = "Upload created";
                }
                else
                {
                    // Edit
                    Source s = await _db.Sources.FindAsync(model.Id);
                    if (s == null)
                    {
                        TempData["error"] = "Can't find record to update";
                        return RedirectToAction(nameof(Index));
                    }
                    await model.UpdateSource(s);
                    await _db.SaveChangesAsync();
                    TempData["message"] = "Update successful";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error - {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            Source s = await _db.Sources.FindAsync(id);
            if (s == null)
            {
                return BadRequest();
            }
            _db.Sources.Remove(s);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
