using SourcemapToolkit.CallstackDeminifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Util
{
    public class StringSourceMapProvider : ISourceMapProvider
    {
        private readonly string _sourceMap;

        public StringSourceMapProvider(string sourceMap)
        {
            _sourceMap = sourceMap;
        }

        public StreamReader GetSourceMapContentsForCallstackUrl(string url)
        {
            // Ignore the url and just return the string we were supplied with
            byte[] byteArray = Encoding.ASCII.GetBytes(_sourceMap);
            MemoryStream stream = new MemoryStream(byteArray);
            return new StreamReader(stream);
        }
    }
}
