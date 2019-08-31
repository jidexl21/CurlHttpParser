using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;

namespace CurlHttpParser
{
    public static class Extensions
    {
        public static void AppendUrlEncoded(this StringBuilder sb, string name, string value)
        {
            if (sb.Length != 0)
                sb.Append("&");
            sb.Append(HttpUtility.UrlEncode(name));
            sb.Append("=");
            sb.Append(HttpUtility.UrlEncode(value));
        }

        public static HttpRequestMessage FromCurlString(this HttpRequest sb, string requestString)
        {
            StringParser parser = new StringParser();
            return parser.CreateHttpRequest(requestString);
        }
    }
}
