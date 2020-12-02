using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccesor;

        public BrowserTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccesor)
        {
            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccesor = actionContextAccesor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext);
            Browser = Browser.ToLower();
            output.TagName = "span";

            if (Browser.Contains("safari"))
            {
                var url = urlHelper.Content("~/images/safari.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Safari {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("firefox"))
            {
                var url = urlHelper.Content("~/images/firefox.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Firefox {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("chrome"))
            {
                var url = urlHelper.Content("~/images/chrome.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Chrome {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("edge"))
            {
                var url = urlHelper.Content("~/images/edge.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Edge {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("samsung internet"))
            {
                var url = urlHelper.Content("~/images/samsung-internet.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Samsung Internet {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("brave"))
            {
                var url = urlHelper.Content("~/images/brave.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Brave {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser.Contains("uc browser"))
            {
                var url = urlHelper.Content("~/images/uc-browser.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"Brave {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else if (Browser == "ie")
            {
                var url = urlHelper.Content("~/images/ie.png");
                output.Content.SetHtmlContent($"<span data-toggle=\"tooltip\" title=\"IE {Version}\"><img width=\"16\" height=\"16\" src=\"{url}\"></img></span>");
            }
            else
            {
                output.Content.SetHtmlContent($"<i class=\"fab fa-question\" title=\"{Browser} {Version}\"></i>");
            }
        }
    }
}
