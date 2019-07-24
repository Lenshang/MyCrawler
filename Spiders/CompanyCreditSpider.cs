using MyCrawler.Http;
using MyCrawler.Model;
using MyCrawler.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Spiders
{
    class CompanyCreditSpider : BaseSpider
    {
        int threshold = 100;//
        public CompanyCreditSpider(): base(maxThread: 1)
        {
            //上海全市企业商铺商家信息爬虫
            //通过获得全上海路名 遍历路名号码方式
        }
        public override void Run()
        {
            string url = "https://xin.baidu.com/s?q=%E9%9E%8D%E5%B1%B1%E6%94%AF%E8%B7%AF&t=3&fl=1&castk=LTE%3D";
            this.http.Get(url,this.ParseRoadIndex);
        }
        public override object BeforeRequest(BaseHttpClient client, RequestEntity request, MetaModel meta)
        {
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            if (request.Url.ToString().Contains("baidu"))
            {
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Referer = "https://xin.baidu.com/s?q=%E9%9E%8D%E5%B1%B1%E6%94%AF%E8%B7%AF&t=3&fl=1&castk=LTE%3D";
            }
            return base.BeforeRequest(client, request, meta);
        }
        private void ParseRoadIndex(HttpContentModel response)
        {
            var letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            foreach (var letter in letters)
            {
                string url = $"http://sh.city8.com/road/{letter}/";
                this.http.Get(url, ParseRoadName);
            }
        }
        private void ParseRoadName(HttpContentModel response)
        {
            var doc=response.response.GetDocument();
            var lists = doc.QuerySelectorAll("div.road_sahngjia.road_zm_list a");
            foreach(var item in lists)
            {
                string name = item.TextContent.Trim();
                string url = $"https://xin.baidu.com/s?q={System.Web.HttpUtility.UrlEncode(name)}&t=0";
                this.http.Get(url, this.ParseCompanyName);
            }
        }
        private void ParseCompanyName(HttpContentModel response)
        {
            var doc = response.response.GetDocument();
            var lists = doc.QuerySelectorAll("div.zx-list-wrap div.zx-list-item");
            foreach(var item in lists)
            {
                Console.WriteLine(item.QuerySelector("h3 a").TextContent);
            }
        }
    }
}
