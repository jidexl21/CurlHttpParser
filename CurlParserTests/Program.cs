using System;
using System.Collections.Generic;
using System.IO;
using CurlHttpParser;
using Newtonsoft.Json; 

namespace CurlParserTests
{
    class Program
    {
        static void Main(string[] args)
        {
            StringParser p = new StringParser();
            Program Tester = new Program();
            var filelst = Directory.EnumerateFiles("C:\\Users\\Olajide Fagbuji\\Documents\\Visual Studio 2017\\Projects\\CurlHttpParser\\CurlParserTests\\Cases", "");
            foreach (string file in filelst)
            {
                string tt = File.ReadAllText(file);
                int end = tt.IndexOf("#####"); 
                tt = (end!=-1) ? tt.Substring(0,tt.IndexOf("#####")) : tt;
                var curr = p.Parse(tt);
                Tester.runChecks(curr, file);

            }
            Console.ReadLine();

        }

        string errors = "";
        string CurrentTest = "";
        string CurrentCase = ""; 
        public void runChecks(ExtractedParams p, string fileName) {

            var checklist = new List<Func<ExtractedParams,bool>>();
            checklist.Add(CheckURL);
            checklist.Add(CheckMethod);
          

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
            Console.WriteLine(errors);
            errors = ""; 

        }

        private bool CheckMethod(ExtractedParams p){
            CurrentTest = "Http Method (Verb)";
            string allowed = "POST_GET_PUT_PATCH_DELETE_";
            string localErrors = "";
            string chkmethod = p.Method ?? "NONE";
            if (string.IsNullOrEmpty(chkmethod)) { localErrors += "No Method Was retrieved\r\n";  }
            if (allowed.IndexOf(chkmethod) == -1) { localErrors += "Not a valid http request method verb\r\n"; }
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

    }
}
