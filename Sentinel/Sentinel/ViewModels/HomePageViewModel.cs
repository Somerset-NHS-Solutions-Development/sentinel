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
        public List<ErrorLog> Errors { get; set; }
        public List<string> Applications { get; set; }
        public List<string> Agents { get; set; }
        public List<string> OsList { get; set; }
        public List<string> Devices { get; set; }
        public bool More { get; set; }
    }
}
