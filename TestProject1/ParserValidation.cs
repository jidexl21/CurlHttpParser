using CurlHttpParser;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Tokens;

namespace CurlHttpParserTests
{

    public class ParserValidation
    {
        private static StringParser parser;
        private static IDeserializer yamlParser;
        private static Dictionary<string, (string,string)> Cases;

        private static string[] Verbs = {
           "POST","GET","PUT","PATCH","DELETE"
        };

        static ParserValidation()
        {
            Cases = LoadAllCases();
            parser = new StringParser();
            yamlParser = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        }


        [Theory]
        [InlineData("Case1.txt")]
        [InlineData("Case2.txt")]
        [InlineData("Case3.txt")]
        [InlineData("Case4.txt")]
        [InlineData("Case7.txt")]
        public void CheckMethod(string file)
        {
            var (given, expected) = Cases[file];
            var data = parser.Parse(given);

            Verbs.Should().Contain(data.Method);
            GetExpected<string>(expected, "METHOD", out string obj);
            data.Method.Should().Be(obj);
        }

        [Theory]
        [InlineData("Case1.txt")]
        [InlineData("Case2.txt")]
        [InlineData("Case3.txt")]
        [InlineData("Case4.txt")]
        [InlineData("Case7.txt")]
        public void CheckURL(string file)
        {
            var (given, expected) = Cases[file];
            var data = parser.Parse(given);
            GetExpected<string>(expected, "URL", out string obj);
            data.URL.Should().Be(obj);
        }

        [Theory]
        [InlineData("Case1.txt")]
        [InlineData("Case2.txt")]
        [InlineData("Case3.txt")]
        [InlineData("Case4.txt")]
        [InlineData("Case7.txt")]
        private void CheckHeaders(string file)
        {
            var (given, expected) = Cases[file];
            var data = parser.Parse(given);
            GetExpected<IEnumerable<object>>(expected, "HEADER", out var obj);

            obj.Should().HaveCount(data.Headers.Count);
        }

        private static (string, string) GetCase(string file)
        {
            string allText = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    allText = reader.ReadToEnd();
                }
            }

            int end = allText.IndexOf("#####");
            string tt = (end != -1) ? allText.Substring(0, allText.IndexOf("#####")) : allText;
            int x = allText.LastIndexOf("#####");
            string exp = (end != -1) ? allText.Substring(x + 5) : "";
            return (tt, exp);
        }

        private static Dictionary<string,(string,string)> LoadAllCases()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var cases = new Dictionary<string, (string,string)>();
            foreach (var resource in assembly.GetManifestResourceNames())
            {
                var Key = resource.Split("Cases.").Last();
                var caseFile = GetCase(resource);
                cases.Add(Key, caseFile);
            }
            return cases;
        }

        private bool GetExpected<T>( string maintext, string Key, out T expected)
        {
            var expectedObj = (Dictionary<object,object>) yamlParser.Deserialize(maintext);
            var result =  expectedObj.FirstOrDefault(x => x.Key.ToString() == Key);
            expected =  (result.Value is null) ? default(T) : (T)result.Value;
            return !(result.Value is null);
        }

        //private bool GetExpected<T>(string maintext, string Key, out IEnumerable<T> expected)
        //{
        //    expected = null;
        //    var expectedObj = (Dictionary<object, object>)yamlParser.Deserialize(maintext);
        //    var result = expectedObj.Where(x => x.Key.ToString() == Key);
        //    expected = (result.Any()) ? result.Select(x => (T)x.Value).ToList() : Enumerable.Empty<T>();
        //    return result.Any();

        //    return false;
        //}

    }
}
