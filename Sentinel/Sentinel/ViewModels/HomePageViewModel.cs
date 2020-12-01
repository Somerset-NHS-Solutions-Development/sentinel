using Microsoft.AspNetCore.Mvc.Rendering;
using Sentinel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class HomePageViewModel
    {
        public List<ErrorLog> errors { get; set; }
        public List<string> applications { get; set; }
        public List<string> agents { get; set; }
        public List<string> osList { get; set; }
        public List<string> devices { get; set; }
        public bool more { get; set; }
    }
}
