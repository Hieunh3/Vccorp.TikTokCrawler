using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCCorp.TikTokCrawler.DAO
{
    public class ConnectionDAO
    {
        /* Server */
        public const string ConnectionToTableLinkProduct = @"Data Source=localhost,3306;Initial Catalog = crawler_preview; User ID = root; Password=hieu123";

        public const string ConnectionToTableSiPost = @"Data Source=192.168.23.22,3306;Initial Catalog = social_index_v2; User ID = hieunh; Password = F4orxjwAM9UWI8fBp65C";

        public const string ConnectionToTableReportDaily = @"Server=localhost;User ID = root;Password=123456aA@;";

        /* Local */
        //public const string ConnectionToTableLinkProduct = @"Server=192.168.23.22;User ID = minhdq;Password=wgy2FdMt0rXfcmCWGSqa;";

        public const string KEY_BOT = "5510917559:AAFc29FuT3isv31rJJ-Hf3U8J3TQm7wUjn8";
        public static readonly long ID_TELEGRAM_BOT_GROUP_COMMENT_ECO = 966893697;

    }
}
