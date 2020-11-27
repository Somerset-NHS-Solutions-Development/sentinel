using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.TagHelpers
{
    public class BrowserTagHelper : TagHelper
    {
        public string Browser { get; set; }
        public string Version { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Browser = Browser.ToLower();
            output.TagName = "span";

            if (Browser.Contains("safari"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-safari\" title=\"Safari {Version}\"></i>");
            }
            else if (Browser.Contains("firefox"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-firefox-browser\" title=\"Firefox {Version}\"></i>");
            }
            else if (Browser.Contains("chrome"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-chrome\" title=\"Chrome {Version}\"></i>");
            }
            else if (Browser.Contains("edge"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-edge\" title=\"Edge {Version}\"></i>");
            }
            else if (Browser == "ie")
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-internet-explorer\" title=\"IE {Version}\"></i>");
            }
            else
            {
                output.Content.SetContent(Browser + " " + Version);
            }
        }
    }
}
