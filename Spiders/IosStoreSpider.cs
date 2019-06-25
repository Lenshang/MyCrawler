using AngleSharp;
using AngleSharp.Html.Parser;
using MyCrawler.Model;
using MyCrawler.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MyCrawler.Spiders
{
    /// <summary>
    /// IOS应用商店网页版爬虫
    /// 抓取IOS应用商店网页版的相关数据
    /// </summary>
    public class IosStoreSpider:BaseSpider
    {
        public IosStoreSpider():base(maxThread:20)
        {
        }
        public override void Run()
        {
            var typeDictionary = new Dictionary<string, string>();
            typeDictionary.Add("图书", "id6018");

            var letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "*" };
            foreach(var key in typeDictionary.Keys)
            {
                string _id = typeDictionary[key];
                foreach(var letter in letters)
                {
                    string url = "https://itunes.apple.com"+$"/cn/genre/ios-{key}/{_id}?mt=8&letter={letter}&page=1#page";
                    Dictionary<string, object> meta = new Dictionary<string, object>();
                    meta.Add("page", 1);
                    this.http.Get(url, ParseAppList, meta);
                    break;
                }
            }
        }

        private void ParseAppList(HttpContentModel response)
        {
            var document = response.response.GetDocument();
            foreach(var item in document.QuerySelectorAll("#selectedcontent div a"))
            {
                var appName = item.TextContent;
                var appUrl = item.GetAttribute("href");
                var meta = CopyHelper.DeepCopy(response.meta);
                meta.Add("appName", appName);
                this.http.Get(appUrl, ParseAppDetail, meta);
            }
            
        }
        private void ParseAppDetail(HttpContentModel response)
        {
            var meta = response.meta as Dictionary<string, object>;
            Console.WriteLine(meta["appName"]);
            IosAppItem appItem = new IosAppItem();
            appItem.AppName = meta["appName"].ToString();
            appItem.AppUrl = response.request.Url.ToString();
            string html = response.response.GetHtml();
            string json1=RegexHelper.RegexOne(html,"class=\"ember-view\" type=\"application/ld\\+json\"\\>([\\s\\S]*?)\\<\\/script\\>");
            string json2= RegexHelper.RegexOne(html, "script type=\"fastboot/shoebox\" id=\"shoebox-ember-data-store\"\\>([\\s\\S]*?)\\<\\/script\\>");
            var jObj1 = JsonConvert.DeserializeObject<JObject>(json1);
            var jObj2= JsonConvert.DeserializeObject<JObject>(json2);
            appItem.Description = jObj1["description"].ToString();
            appItem.AppType= jObj1["applicationCategory"].ToString();
            appItem.FirstReleaseDate= jObj1["datePublished"].ToString();
            appItem.Producer= jObj1["author"]?["name"]?.ToString();
            appItem.Star= jObj1["aggregateRating"]?["ratingValue"]?.ToString();
            appItem.FeedbackRate = appItem.Star;
            appItem.FeedbackCount= jObj1["aggregateRating"]?["reviewCount"]?.ToString();
            appItem.CommentCount = appItem.FeedbackCount;
            appItem.Price= jObj1["offers"]?["price"]?.ToString();
            appItem.Size = jObj2["data"]?["attributes"]?["size"]?.ToString();
            appItem.ReleaseDate= jObj2["data"]?["attributes"]?["versionHistory"]?[0]?["releaseDate"]?.ToString();
            appItem.VersionNumber = jObj2["data"]?["attributes"]?["versionHistory"]?[0]?["versionString"]?.ToString();
            appItem.ImgUrl= jObj1["image"]?.ToString();
            //TODO 写入数据库
        }
        public override object BeforeRequest(BaseHttpClient client, RequestEntity request, Dictionary<string, object> meta)
        {
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            return base.BeforeRequest(client, request,meta);
        }

        public override object BeforeException(BaseHttpClient client, Exception e, RequestEntity request, ResponseEntity response, Dictionary<string, object> meta)
        {
            if(e is CatchHttpCodeException)
            {
                return request;//重新请求
            }
            return base.BeforeException(client, e, request, response,meta);
        }
    }

    public class IosAppItem
    {
        public string AppName { get; set; }
        public string AppUrl { get; set; }
        public string Description { get; set; }
        public string AppType { get; set; }
        public string FirstReleaseDate { get; set; }
        public string Producer { get; set; }
        public string Star { get; set; }
        public string FeedbackRate { get; set; }
        public string FeedbackCount { get; set; }
        public string CommentCount { get; set; }
        public string Price { get; set; }
        public string Size { get; set; }
        public string ReleaseDate { get; set; }
        public string VersionNumber { get; set; }
        public string ImgUrl { get; set; }
        public long CrawlDate { get; set; }

    }
}
