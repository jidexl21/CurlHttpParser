using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace CurlHttpParser
{
    public class StringParser
    {

        public HttpRequest CreateHttpRequest(string RequestString) {
            var details = Parse(RequestString);
            var request = new HttpRequestMessage(new HttpMethod(details.Method), details.URL);
            foreach (var hdr in details.Headers) {

            }
            foreach (var content in details.Data) {
                request.Content = new StringContent((string) content, Encoding.UTF8);
            }
            return null;
        }

        public ExtractedParams Parse(string RequestString)
        {
            RequestString = RequestString.Replace("\\\r\n", "").Replace("\r\n", "").Replace("\n", "");
            ExtractedParams p = new ExtractedParams();
            bool hasPostData = false;
            StringBuilder postData = new StringBuilder();

            List<Dictionary<string, string>> x = new List<Dictionary<string, string>>();
            string[] options = RequestString.Split(new string[] { " --", " -" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in options) {
                bool matchd = false; 
                int delimiter = item.IndexOf(" ");
                Dictionary<string, string> itm = new Dictionary<string, string>();
                string trimmed = item.Trim(new char[] { '\'', ' ' });
                if (delimiter <= 0) {
                    if (trimmed.IndexOf("http") != 0) { p.URL = trimmed; continue;}
                    continue;
                };
                string key = item.Substring(0, delimiter).Trim(new char[] { '-'});
                string value = item.Substring(delimiter).Trim();
                switch (key.ToLower()) {
                    case "header":
                    case "h":
                        p.Headers.Add(value); matchd = true;
                        break;
                    case "data":
                    case "d":
                        p.Data.Add(value); matchd = true;
                        break;
                    case "url":
                        p.URL = value; matchd = true;
                        break;
                    case "request":
                    case "x":
                        int notClean = value.IndexOf(" ");
                        p.Method = (notClean == -1) ? value : value.Substring(0,notClean); matchd = true;
                        break;
                    case "f":
                        string[] kv = value.Split("=");
                        postData.AppendUrlEncoded(kv[0].Trim(new char[] { '\'' }), kv[1].Trim(new char[]{ '\''})); hasPostData = true; matchd = true;
                        break;
                    case "u":
                        var dict = new Dictionary<string, string>();
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
                        dict.Add("Authorization", $"Basic {base64authorization}");
                        p.Headers.Add(dict);
                        matchd = true;
                        //request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
                        break;
                };
                if (!matchd) {
                    if (trimmed.IndexOf("http") != -1) { p.URL = trimmed.Substring(trimmed.IndexOf("http")); continue; }                   
                }
                string pattern = @"(http|smtp|https):\/\/([\w,.,\/,-?=])*";
                Regex rg = new Regex(pattern);
                if (!rg.IsMatch(p.URL)) { p.URL = ""; }
                if (string.IsNullOrEmpty(p.URL))
                {
                    MatchCollection found = rg.Matches(trimmed);
                    if (found.Count > 0) { p.URL = found[0].Value; }
                }
            };

            if (hasPostData){
                p.Data.Add(postData.ToString());
            }
           
            //return x;
            return p; 
        }

         
    }
}
