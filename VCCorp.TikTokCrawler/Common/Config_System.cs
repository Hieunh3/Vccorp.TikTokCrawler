using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCCorp.TikTokCrawler.Common
{
    public class Config_System
    {
        public const int Status = 0; // cột status của table si_demand_post

        public const string Platform = "tiktok"; // cột platform của table si_demand_post

        public static readonly string SERVER_LINK = "10.3.48.81:9092,10.3.48.90:9092,10.3.48.91:9092";
        public static readonly string TOPIC_TIKTOK_POST = "crawler-data-tiktok";
    }
}
