using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace VCCorp.TikTokCrawler.Common
{
    public static class Extensions
    {
        public static readonly JsonSerializerOptions opt = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            WriteIndented = true
        };
        public static string ToJson<T>(T obj)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize<T>(obj, opt);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
