using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.ViewModels
{
    public class ViewCodeModel
    {
        public string Url { get; set; }
        public string Code { get; set; }
        public string Lang { get; set; }
        public int Line { get; set; }
        public bool Unminified { get; set; }
    }
}
