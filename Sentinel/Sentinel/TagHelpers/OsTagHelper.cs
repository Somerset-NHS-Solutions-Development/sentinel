using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.TagHelpers
{
    public class OsTagHelper : TagHelper
    {
        public string Os { get; set; }
        public string Version { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Os = Os.ToLower();
            output.TagName = "span";

            if (Os.Contains("windows"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-windows text-primary\" title=\"Windows {Version}\"></i>");
            }
            else if (Os.Contains("ios"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-apple text-secondary\" title=\"iOS {Version}\"></i>");
            }
            else if (Os.Contains("android"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-android text-success\" title=\"Android {Version}\"></i>");
            }
            else if (Os.Contains("linux"))
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-linux\" title=\"Linux {Version}\"></i>");
            }
            else
            {
                output.Content.SetContent(Os + " " + Version);
            }
        }
    }
}
