using System;
using System.Collections.Generic;

#nullable disable

namespace Sentinel.Models
{
    public partial class ErrorLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ClientIp { get; set; }
        public string Application { get; set; }
        public string UserAgent { get; set; }
        public string UserAgentVersion { get; set; }
        public string Os { get; set; }
        public string Osversion { get; set; }
        public string Device { get; set; }
        public string VueInfo { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
        public string StackTrace { get; set; }
        public bool Processed { get; set; }
    }
}
