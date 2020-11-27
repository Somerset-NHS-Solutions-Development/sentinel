using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Configuration
{
    public class ApplicationOptions
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string EmailFrom { get; set; }
        public string EMailCc { get; set; }
        public string EMailBcc { get; set; }

    }
}
