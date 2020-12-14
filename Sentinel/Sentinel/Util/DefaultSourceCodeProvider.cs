using SourcemapToolkit.CallstackDeminifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentinel.Util
{
    public class DefaultSourceCodeProvider : ISourceCodeProvider
    {
        private readonly HttpClient _httpClient;

        public DefaultSourceCodeProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public StreamReader GetSourceCode(string sourceFileUrl)
        {
            var response = _httpClient.GetAsync(sourceFileUrl).Result;
            var stream = response.Content.ReadAsStreamAsync().Result;

            return new StreamReader(stream);
        }
    }
}
