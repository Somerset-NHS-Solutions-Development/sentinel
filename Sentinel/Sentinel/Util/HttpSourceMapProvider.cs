using SourcemapToolkit.CallstackDeminifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentinel.Util
{
    public class HttpSourceMapProvider : ISourceMapProvider
    {
        private readonly HttpClient _httpClient;

        public HttpSourceMapProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public StreamReader GetSourceMapContentsForCallstackUrl(string codeUrl)
        {
            // There seems to be no standard naming convention for source maps, 
            // so a source map for foo.min.js could be named foo.min.map OR foo.min.js.map
            var sourceMapUrl = codeUrl.Replace(".min.js", ".min.map");
            var response = _httpClient.GetAsync(sourceMapUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                // Try including the ".js"
                sourceMapUrl = codeUrl.Replace(".min.js", ".min.js.map");
                response = _httpClient.GetAsync(sourceMapUrl).Result;
            }
            var stream = response.Content.ReadAsStreamAsync().Result;

            return new StreamReader(stream);
        }
    }
}
