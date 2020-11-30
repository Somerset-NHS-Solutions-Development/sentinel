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
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Windows {Version}\"><i class=\"fab fa-windows text-primary\"></i></span>");
            }
            else if (Os.Contains("ios"))
            {
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"iOS {Version}\"><i class=\"fab fa-apple text-secondary\"></i></span>");
            }
            else if (Os.Contains("android"))
            {
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Android {Version}\"><i class=\"fab fa-android text-success\"></i></span>");
            }
            else if (Os.Contains("linux"))
            {
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Linux {Version}\"><i class=\"fab fa-linux\"></i></span>");
            }
            else
            {
                output.Content.SetContent(Os + " " + Version);
            }
        }
    }
}
