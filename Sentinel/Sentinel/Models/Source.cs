using System;
using System.Collections.Generic;

#nullable disable

namespace Sentinel.Models
{
    public partial class Source
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Filename { get; set; }
        public string MapSource { get; set; }
        public string FullSource { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
