using CefSharp.WinForms;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VCCorp.CrawlerCore.BUS;
using VCCorp.CrawlerCore.Common;
using VCCorp.TikTokCrawler.Common;
using VCCorp.TikTokCrawler.DAO;
using VCCorp.TikTokCrawler.Model;
using static System.Windows.Forms.LinkLabel;

namespace VCCorp.TikTokCrawler.Controller
{
    public class TikTokHashTagController
    {
        private ChromiumWebBrowser _browser = null;
        private readonly HtmlAgilityPack.HtmlDocument _document = new HtmlAgilityPack.HtmlDocument();
        private string URL_VINAMILK = "https://www.tiktok.com/search?q=vinamilk";
        private string URL_TIKTOK = "https://www.tiktok.com/";
        private const string _jsAutoScroll = @"window.scrollTo(0, document.body.scrollHeight)/3";
        private const string _jsLoadMore = @"document.getElementsByClassName('tiktok-154bc22-ButtonMore')[0].click()";

        public TikTokHashTagController(ChromiumWebBrowser browser)
        {
            _browser = browser;
        }

        public async Task CrawlData()
        {
            await NewLinkTiTok_Db(URL_VINAMILK);
        }
        /// <summary>
        /// Thêm url vào tiktok_link table.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<TikTokDTO>> NewLinkTiTok_Db(string url)
        {
            List<TikTokDTO> tiktokPost = new List<TikTokDTO>();
            ushort indexLastContent = 0;
            try
            {
                await _browser.LoadUrlAsync(url);
                await Task.Delay(20_000);
                byte i = 0;
                while (i < 20)
                {
                    i++;
                    string html = await Common.Utilities.GetBrowserSource(_browser).ConfigureAwait(false);
                    _document.LoadHtml(html);
                    html = null;

                    HtmlNodeCollection divComment = _document.DocumentNode.SelectNodes($"//div[contains(@class,'tiktok-1soki6-DivItemContainerForSearch')][position()>{indexLastContent}]");
                    if (divComment == null)
                    {
                        break;
                    }
                    if (divComment != null)
                    {
                        foreach (HtmlNode item in divComment)
                        {
                            string urlVid = item.SelectSingleNode(".//div[contains(@class,'tiktok-yz6ijl-DivWrapper')]/a")?.Attributes["href"].Value;
                            string idVid = Regex.Match(urlVid, @"(?<=/video/)\d+").Value; // lấy id_post

                            TikTokDTO content = new TikTokDTO();
                            content.link = urlVid;
                            content.domain = URL_VINAMILK;
                            content.post_id = idVid;
                            DateTime createDate = DateTime.Now;
                            string postDate = item.SelectSingleNode(".//div[contains(@class,'tiktok-842lvj-DivTimeTag')]")?.InnerText;
                            if (!string.IsNullOrEmpty(postDate))
                            {
                                Common.DateTimeFormatAgain dtFomat = new Common.DateTimeFormatAgain();
                                string date = dtFomat.GetDateBySearchText(postDate, "yyyy-MM-dd HH:mm:ss");
                                try
                                {
                                    createDate = Convert.ToDateTime(date);
                                }
                                catch { }
                            }
                            content.create_time = createDate; // ngày tạo vid
                            content.post_id = idVid; //id video
                            content.platform = Common.Config_System.Platform;
                            content.crawled_time = DateTime.Now; // thời gian bóc
                            content.update_time = createDate; // thời gian update
                            content.status = Common.Config_System.Status;

                            tiktokPost.Add(content);

                            //Lấy vid từ tháng 11
                            if (createDate > DateTime.Now.AddDays(-37))
                            {
                                //lưu vào db si_demand_source
                                TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableSiPost);
                                await msql.InserToSiPostTable(content);
                                //TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                                //await msql.InserTikTokUrlTable(content);
                                msql.Dispose();

                                #region gửi đi cho ILS
                                Tiktok_Post_Kafka_Model kafka = new Tiktok_Post_Kafka_Model();
                                kafka.IdVideo = content.post_id;
                                kafka.UserName = item.SelectSingleNode(".//p[contains(@class,'tiktok-2zn17v-PUniqueId etrd4pu6')]")?.InnerText;
                                //kafka.IdUser = "@"+kafka.UserName;
                                kafka.UrlUser = URL_TIKTOK +"@"+ kafka.UserName;
                                kafka.Avatar = item.SelectSingleNode(".//span[contains(@class,'tiktok-tuohvl-SpanAvatarContainer')]//img")?.Attributes["src"]?.Value ?? "";
                                kafka.Content = Common.Utilities.RemoveSpecialCharacter(item.SelectSingleNode(".//div[contains(@class,'tiktok-1ejylhp-DivContainer')]/span[contains(@class,'tiktok-j2a19r-SpanText')][1]")?.InnerText);
                                kafka.LinkVideo = urlVid;

                                //tối ưu playCount về dạng int
                                string numString = item.SelectSingleNode(".//strong[contains(@class,'tiktok-ws4x78-StrongVideoCount')]")?.InnerText;
                                string chu = Regex.Match(numString, @"\D$").Value;// trường hợp có cả chữ cả số (10k)
                                int so = Int32.Parse(Regex.Match(numString, @"\d+").Value);//trường hợp chỉ có số (1234)
                                
                                if (chu == "K")
                                {
                                    kafka.PlayCounts = so * 1000;
                                }
                                else if (chu=="M")
                                {
                                    kafka.PlayCounts = so * 1000000;
                                }
                                else if(chu == "")
                                {
                                    kafka.PlayCounts = so; 
                                }
                                kafka.TimePost = createDate;
                                kafka.TimePostTimeStamp = (double)(Date_Helper.ConvertDateTimeToTimeStamp(createDate));
                                kafka.TimeCreated = DateTime.Now;
                                kafka.TimeCreateTimeStamp = (double)(Date_Helper.ConvertDateTimeToTimeStamp(DateTime.Now));

                                string jsonPost = ToJson<Tiktok_Post_Kafka_Model>(kafka);                              
                                Kafka_Helper kh = new Kafka_Helper();
                                //await kh.InsertPost(jsonPost, "crawler-data-tiktok");
                                #endregion
                            }

                            indexLastContent++;
                        }
                    }
                    //check JS roll xuống cuối trang
                    string checkJs = await Common.Utilities.EvaluateJavaScriptSync(_jsLoadMore, _browser).ConfigureAwait(false);
                    if (checkJs == null)
                    {
                        break;
                    }
                    await Task.Delay(10_000);
                }
            }
            catch { }
            return tiktokPost;
        }

        public  string ToJson<T>(T obj)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize<T>(obj);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
