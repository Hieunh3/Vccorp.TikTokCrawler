using CefSharp;
using CefSharp.WinForms;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCCorp.TikTokCrawler.Controller;

namespace VCCorp.TikTokCrawler
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser _browser = null;
        public Form1()
        {
            InitializeComponent();
            InitBrowser("https://www.tiktok.com/");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                InitBrowser("https://www.tiktok.com/");
                await Task.Delay(10_000);
                TiktokPostController tiktokPostController = new TiktokPostController(_browser);
                await tiktokPostController.CrawlData();

            }
            catch (Exception)
            {
            }
            await Task.Delay(TimeSpan.FromHours(1));
        }
        public void InitBrowser(string urlBase)
        {
            if (_browser == null)
            {
                this.WindowState = FormWindowState.Maximized;
                CefSettings s = new CefSettings();

                Cef.Initialize(s);
                _browser = new ChromiumWebBrowser(urlBase);
                this.panel1.Controls.Add(_browser);
                _browser.Dock = DockStyle.Fill;
            }
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                InitBrowser("https://www.tiktok.com/");
                await Task.Delay(10_000);
                TikTokHashTagController tiktokPostController = new TikTokHashTagController(_browser);
                await tiktokPostController.CrawlData();

            }
            catch (Exception)
            {
            }
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}
