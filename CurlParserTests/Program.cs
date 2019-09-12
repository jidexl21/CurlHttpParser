using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CurlHttpParser;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; 

namespace CurlParserTests
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] CasesToRun = new string[] { "Case7.txt" };
            StringParser p = new StringParser(); 
            Program Tester = new Program();
            var filelst = Directory.EnumerateFiles("C:\\Users\\Olajide Fagbuji\\Documents\\Visual Studio 2017\\Projects\\CurlHttpParser\\CurlParserTests\\Cases", "");
            foreach (string file in filelst)
            {
                var currFile = Path.GetFileName(file);
                int ShouldRun = CasesToRun.Where( o => (currFile.IndexOf(o) != -1)).Select(o => o).Count();
                
                if(ShouldRun < 1) { continue; }
                //if (Array.IndexOf(CasesToRun, Path.GetFileName(file)) == -1) { continue;  }
                string allText = File.ReadAllText(file); 
                int end = allText.IndexOf("#####"); 
                string tt = (end!=-1) ? allText.Substring(0,allText.IndexOf("#####")) : allText;
                int x = allText.LastIndexOf("#####");
                string exp = (end != -1) ? allText.Substring(x+5) : "";
                var curr = p.Parse(tt);
              
                Tester.runChecks(curr, file, exp);

            }
            Console.ReadLine();

        }

        string errors = "";
        string CurrentTest = "";
        string CurrentCase = "";
        List<string> expected = new List<string>();
        public void runChecks(ExtractedParams p, string fileName, string results) {

            var checklist = new List<Func<ExtractedParams, bool>>();
            checklist.Add(CheckURL);
            checklist.Add(CheckMethod);
            checklist.Add(CheckData);
            checklist.Add(CheckHeaders);
            checklist.Add(CheckRequest);

            expected = new List<string>(results.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
          

            foreach (var check in checklist) {
                string CaseFile = Path.GetFileName(fileName);
                if (CaseFile != CurrentCase) { Console.WriteLine(CaseFile); }
                CurrentCase = CaseFile; 
                if (!check(p))
                {
                    Console.WriteLine($"Failed:  {CurrentTest}");

                }
                else {
                     Console.WriteLine($"Passed: {CurrentTest}");
                }
            }
            if(errors.Length >0) Console.WriteLine("Errors:");
            Console.WriteLine(errors);
            errors = "";


        }

        private bool CheckMethod(ExtractedParams p){
            CurrentTest = "Http Method Validation";
            string allowed = "POST_GET_PUT_PATCH_DELETE_";
            string localErrors = "";
            string chkmethod = p.Method ?? "NONE";
            if (string.IsNullOrEmpty(chkmethod)) { localErrors += "No Method Was retrieved\r\n";  }
            if (allowed.IndexOf(chkmethod) == -1) { localErrors += "Not a valid http request method verb\r\n"; }
            //Console.WriteLine(JsonConvert.SerializeObject(p));
            errors += localErrors;
            
            return (localErrors.Length == 0);
        }

        private bool CheckURL(ExtractedParams p)
        {
            CurrentTest = "URL Validation";
            string localErrors = "";
            string chkURL = p.URL ?? "";
            if (string.IsNullOrEmpty(chkURL)) { localErrors += "No URL Was retrieved\r\n"; return false; }
            if (p.URL.IndexOf("http") != 0) { localErrors += $"Invalid Url ({p.URL})\r\n"; }
            errors += localErrors;
            return (localErrors.Length == 0);
        }

        private bool CheckData(ExtractedParams p){
            CurrentTest = "Data Validation";
            string localErrors = "";
            var da= expected.Where(x => x.Trim().IndexOf("DATA") == 0);
            var data = expected.Where(x => x.Trim().IndexOf("DATA") == 0).Select(x => x.Substring(x.IndexOf(":")+1).Trim()).ToList();
            int matches = p.Data.Select(x => x.ToString()).Intersect(data).ToList().Count() ;
            if (matches != p.Data.Count) { localErrors += $"{matches} data sections found out of expected {data.Count}\r\n"; }
            errors += localErrors;
            return (localErrors.Length == 0); 
        }

        private bool CheckHeaders(ExtractedParams p) {
            CurrentTest = "Header Validation";
            string localErrors = "";
            var data = expected.Where(x => x.Trim().IndexOf("HEADER") == 0).Select(x => x.Substring(x.IndexOf(":")+1).Trim()).ToList();
            int matches = p.Headers.Select(x => x.ToString()).Intersect(data).ToList().Count();
            if (matches != p.Headers.Count) { localErrors += $"{matches} header sections found out of expected {data.Count}\r\n"; }
            errors += localErrors;
            return (localErrors.Length == 0);
        }

        private bool CheckRequest(ExtractedParams p)
        {
            bool success = false;
            CurrentTest = "Request Validation";
            string localErrors = "";
            StringParser parser = new StringParser();
            var request = parser.CreateHttpRequest(p.RawCurl);
            Task<bool> t = TestRequest(request);
            t.Wait();
            success = t.Result;
            errors += localErrors;
            if (!success)
            {
                foreach (var item in p.Data)
                {
                    Console.WriteLine(item);
                }
                //Console.WriteLine(JsonConvert.SerializeObject(p.Data)); 
            }
            else {

            }
            return success; 
        }

        private async Task<bool> TestRequest(HttpRequestMessage request)
        {
            bool success = false;
            HttpClient client = new HttpClient();
            var result = await client.SendAsync(request);
            string resp = await result.Content.ReadAsStringAsync();
            try{
                result.EnsureSuccessStatusCode();
                success = true; 
            }
            catch(Exception e)
            {
                Console.WriteLine(result.ReasonPhrase);
                Console.WriteLine(resp);
                success = false;
            }
            return success;
        }
    }
}
