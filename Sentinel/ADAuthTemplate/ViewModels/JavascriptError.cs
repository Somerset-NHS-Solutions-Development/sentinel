using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class JavascriptError
    {
        public string applicationName { get; set; }
        public string message { get; set; }
        public string source { get; set; }
        public int lineno { get; set; }
        public int colno { get; set; }
        public string stack { get; set; }
        public string vueinfo { get; set; }
        public string agent { get; set; }
        public string platform { get; set; }
    }
}
