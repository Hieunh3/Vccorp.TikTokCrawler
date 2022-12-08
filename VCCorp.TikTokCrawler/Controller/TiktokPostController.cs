using CefSharp.WinForms;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VCCorp.TikTokCrawler.Common;
using VCCorp.TikTokCrawler.DAO;
using VCCorp.TikTokCrawler.Model;

namespace VCCorp.TikTokCrawler.Controller
{
    public class TiktokPostController
    {
        private ChromiumWebBrowser _browser = null;
        private readonly HtmlAgilityPack.HtmlDocument _document = new HtmlAgilityPack.HtmlDocument();
        private string URL_KINGLIVE = "https://www.tiktok.com/@kinglive.vn";
        private const string _jsAutoScroll = @"window.scrollTo(0, document.body.scrollHeight)/3";
        private string path = "D:\\Test\\LastDatePost.txt";

        public TiktokPostController(ChromiumWebBrowser browser)
        {
            _browser = browser;
        }

        public async Task CrawlData()
        {
            if (!File.Exists(path))
            {
                File.Create(path);

            }
            await NewLinkTiTok_Db(URL_KINGLIVE);
            await NewDetailByUrl_Db();
        }

        /// <summary>
        /// Lấy kafka content TikTok
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<Tiktok_Post_Kafka_Model>> NewLinkTiTok_Kafka(string url)
        {
            List<Tiktok_Post_Kafka_Model> tiktokPost = new List<Tiktok_Post_Kafka_Model>();
            ushort indexLastContent = 0;
            try
            {
                await _browser.LoadUrlAsync(url);
                await Task.Delay(10_000);
                while (true)
                {
                    string html = await Utilities.GetBrowserSource(_browser).ConfigureAwait(false);
                    _document.LoadHtml(html);
                    html = null;

                    string userName = _document.DocumentNode.SelectSingleNode("//div[contains(@class,'tiktok-1hdrv89-DivShareTitleContainer')]/h2")?.InnerText;
                    string avartar = _document.DocumentNode.SelectSingleNode(".//img")?.Attributes["src"]?.Value ?? "";

                    HtmlNodeCollection divComment = _document.DocumentNode.SelectNodes($"//div[contains(@class,'tiktok-x6y88p-DivItemContainerV2')][position()>{indexLastContent}]");
                    if (divComment == null)
                    {
                        break;
                    }
                    if (divComment != null)
                    {
                        foreach (HtmlNode item in divComment)
                        {
                            string urlVid = item.SelectSingleNode(".//div[contains(@class,'tiktok-yz6ijl-DivWrapper')]/a")?.Attributes["href"].Value;
                            string idVid = Regex.Match(urlVid, @"(?<=/video/)\d+").Value;

                            Tiktok_Post_Kafka_Model content = new Tiktok_Post_Kafka_Model();
                            content.IdVideo = idVid;
                            content.Content = item.SelectSingleNode(".//div[contains(@class,'eih2qak1')]/a")?.Attributes["title"]?.Value ?? "";
                            content.LinkVideo = urlVid;
                            content.UserName = userName;
                            content.IdUser = userName;
                            content.UrlUser = url;
                            content.Avatar = avartar;
                            //content.Likes = int.Parse(item.SelectSingleNode(".//span[contains(@class,'_17p6nbba')]").InnerText);
                            content.CommentCounts = 0;
                            content.Shares = 0;
                            content.PlayCounts = int.Parse(item.SelectSingleNode(".//strong[contains(@class,'video-count')]").InnerText);
                            content.TimePost = DateTime.Now;
                            content.TimeCreateTimeStamp = 0;
                            content.TimeCreated = DateTime.Now;
                            content.TimeCreateTimeStamp = Utilities.DateTimeToUnixTimestamp(DateTime.Now);
                            tiktokPost.Add(content);

                            TikTokPostDAO msql1 = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                            await msql1.InserToTikTokPostTable(content);
                            msql1.Dispose();

                            indexLastContent++;

                        }
                    }

                }
            }

            catch { }
            return tiktokPost;
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
                await Task.Delay(10_000);
                byte i = 0;
                while (i < 5)
                {
                    i++;
                    string html = await Utilities.GetBrowserSource(_browser).ConfigureAwait(false);
                    _document.LoadHtml(html);
                    html = null;

                    HtmlNodeCollection divComment = _document.DocumentNode.SelectNodes($"//div[contains(@class,'tiktok-x6y88p-DivItemContainerV2')][position()>{indexLastContent}]");
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
                            content.domain = URL_KINGLIVE;
                            content.post_id = idVid;

                            tiktokPost.Add(content);

                            //Trường hợp chưa có file lưu max post_id, max create_date
                            if (new FileInfo(path).Length == 0)
                            {
                                TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                                await msql.InserTikTokLinkTable(content);
                                msql.Dispose();
                            }
                            else
                            {
                                string contentFile = File.ReadAllText(path);
                                string[] items = contentFile.Split('|');
                                TikTokDTO getTextFile = new TikTokDTO(items[0], DateTime.Parse(items[1]));//lấy object trong file .txt
                                string postid = getTextFile.post_id;
                                if (double.Parse(postid) < double.Parse(idVid))//lấy id lớn hơn
                                {
                                    TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                                    await msql.InserTikTokLinkTable(content);
                                    msql.Dispose();
                                }
                            }
                            indexLastContent++;
                        }
                    }
                    //check JS roll xuống cuối trang
                    string checkJs = await Common.Utilities.EvaluateJavaScriptSync(_jsAutoScroll, _browser).ConfigureAwait(false);
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

        /// <summary>
        /// Lấy details từ url bảng tiktok_link.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<TikTokDTO>> NewDetailByUrl_Db()
        {
            List<TikTokDTO> tiktokPost = new List<TikTokDTO>();
            try
            {   //lấy url trong db
                TikTokPostDAO contentDAO = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                List<TikTokDTO> dataUrl = contentDAO.GetLinkByDomain(URL_KINGLIVE);
                contentDAO.Dispose();
                for (int i = 0; i < dataUrl.Count; i++)
                {
                    string url = URL_KINGLIVE + dataUrl[i].link;
                    string link = dataUrl[i].link; //lấy link từ db
                    string idVid = Regex.Match(url, @"(?<=/video/)\d+").Value; // lấy id_post

                    await _browser.LoadUrlAsync(url);
                    await Task.Delay(10_000);

                    string html = await Utilities.GetBrowserSource(_browser).ConfigureAwait(false);
                    _document.LoadHtml(html);
                    html = null;

                    TikTokDTO tikTokDTO = new TikTokDTO();
                    DateTime createDate = DateTime.Now;
                    string postDate = _document.DocumentNode.SelectSingleNode("//span[contains(@class,'e17fzhrb2')]/span[2]")?.InnerText;
                    if (!string.IsNullOrEmpty(postDate))
                    {
                        DateTimeFormatAgain dtFomat = new DateTimeFormatAgain();
                        string date = dtFomat.GetDateBySearchText(postDate, "yyyy-MM-dd HH:mm:ss");
                        try
                        {
                            createDate = Convert.ToDateTime(date);
                        }
                        catch { }
                    }
                    tikTokDTO.create_time = createDate; // ngày tạo vid
                    tikTokDTO.link = link; // link vid
                    tikTokDTO.post_id = idVid; //id video
                    tikTokDTO.platform = Common.Config_System.Platform;
                    tikTokDTO.crawled_time = DateTime.Now; // thời gian bóc
                    tikTokDTO.update_time = createDate; // thời gian update
                    tikTokDTO.status = Common.Config_System.Status;
                    tiktokPost.Add(tikTokDTO);

                    //Trường hợp chưa có file lưu max post_id, max create_date
                    if (new FileInfo(path).Length == 0)
                    {
                        if (DateTime.Now.AddDays(-7) < createDate)
                        {
                            //TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableSiPost);
                            //await msql.InserToSiPostTable(tikTokDTO);
                            //msql.Dispose();

                            TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                            await msql.InserTikTokUrlTable(tikTokDTO);
                            msql.Dispose();
                            TikTokPostDAO msql1 = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                            await msql1.TruncateLink_Db();
                            msql.Dispose();
                        }
                    }
                    else
                    {
                        string content = File.ReadAllText(path);
                        string[] items = content.Split('|');
                        TikTokDTO getTextFile = new TikTokDTO(items[0], DateTime.Parse(items[1]));//lấy object trong file .txt
                        string postid = getTextFile.post_id;

                        if (double.Parse(postid) < double.Parse(idVid))//lấy id lớn hơn
                        {
                            TikTokPostDAO msql = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                            await msql.InserTikTokUrlTable(tikTokDTO);
                            msql.Dispose();
                            TikTokPostDAO msql1 = new TikTokPostDAO(ConnectionDAO.ConnectionToTableLinkProduct);
                            await msql1.TruncateLink_Db();
                            msql.Dispose();
                        }
                    }
                }
                DateTime lastDate = tiktokPost.Max(x => x.create_time);//lấy thời gian post gần nhất (lớn nhất)               
                string post_id = tiktokPost.FirstOrDefault(x => x.create_time == lastDate)?.post_id ?? "";
                File.WriteAllText(path, post_id + "|" + lastDate.ToString("yyyy-MM-dd HH:mm:ss")); //lưu vào file
            }
            catch { }
            return tiktokPost;
        }
    }
}

