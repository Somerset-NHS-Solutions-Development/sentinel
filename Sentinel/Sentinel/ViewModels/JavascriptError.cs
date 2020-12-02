using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class JavascriptError
    {
        public string applicationName { get; set; }
        // Define these two as JTokens as datatables puts JSON into these fields
        public JToken message { get; set; }
        public JToken source { get; set; }
        public string lineno { get; set; }
        public string colno { get; set; }
        public string stack { get; set; }
        public string vueinfo { get; set; }
        public string agent { get; set; }
        public string platform { get; set; }
    }
}
