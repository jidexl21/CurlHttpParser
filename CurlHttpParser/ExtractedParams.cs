using System;
using System.Collections.Generic;
using System.Text;

namespace CurlHttpParser
{
    public class ExtractedParams
    {
        public string Method { get; set; }
        public string URL { get; set; }
        public List<Dictionary<string, string>> Headers { get; set; } = new List<Dictionary<string, string>>();
        public List<object> Data { get; set; } = new List<object>();
        public string RawCurl { get; set; }

    }
}
