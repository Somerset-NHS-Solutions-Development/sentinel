using Microsoft.AspNetCore.Http;
using Sentinel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class EditUploadViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Application name")]
        public string Application { get; set; }
        [Required]
        [Display(Name = "Name of minified .js file")]
        public string Filename { get; set; }
        public IFormFile SourceMap { get; set; }
        public IFormFile FullSource { get; set; }

        public EditUploadViewModel() { }

        public EditUploadViewModel (Source s)
        {
            Id = s.Id;
            Application = s.Application;
            Filename = s.Filename.Replace(".min.js", "");
        }

        public async Task UpdateSource(Source s)
        {
            s.Application = Application;
            s.Filename = Filename + ".min.js";
            s.LastUpdated = DateTime.UtcNow;
            if (SourceMap != null)
            {
                using var stream = SourceMap.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                s.MapSource = await streamReader.ReadToEndAsync();
            }
            if (FullSource != null)
            {
                using var stream = FullSource.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                s.FullSource = await streamReader.ReadToEndAsync();
            }
        }
    }
}
