using Sentinel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class UploadSummaryViewModel
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Filename { get; set; }
        public DateTime LastUpdated { get; set; }

        public UploadSummaryViewModel(Source s)
        {
            Id = s.Id;
            Application = s.Application;
            Filename = s.Filename;
            LastUpdated = s.LastUpdated;
        }
    }
}
