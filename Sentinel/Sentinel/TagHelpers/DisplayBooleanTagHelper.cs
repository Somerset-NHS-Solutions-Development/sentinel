using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.TagHelpers
{
    public class DisplayBooleanTagHelper : TagHelper
    {
        public bool Value { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            if (Value)
            {
                output.Content.SetHtmlContent("<i class=\"fas fa-check\"></i>");
            }
            else
            {
                output.Content.SetHtmlContent("<i class=\"fas fa-times\"></i>");
            }
        }
    }
}
