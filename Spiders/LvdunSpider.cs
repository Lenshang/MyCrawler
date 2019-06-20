using AngleSharp;
using AngleSharp.Html.Parser;
using MyCrawler.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace LvdunCrawler
{
    class LvdunSpider
    {
        public HttpEventClient http;
        object locker = new object();
        //int maxThread = 1;
        //int currentThread = 0;
        IHtmlParser parser;
        bool debug = false;
        public LvdunSpider()
        {
            this.http = new HttpEventClient(1);
            
            this.http.RegResponsePipelines(this.BeforeResponse);
            
            this.http.RegRequestPipelines((client,request) => {
                request.Headers.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
                return request;
            });
            this.http.RegRequestPipelines(this.BeforeRequest);
            var config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            parser = context.GetService<IHtmlParser>();
        }
        public void Run()
        {
            int page = 1;
            int maxPage = 5000;
            while (true)
            {

                string url = $"http://www.11315.com/al/vl-{page}";
                this.http.Get(url, ParseListpage);
                if (this.debug)
                {
                    break;
                }
                if (page >= maxPage)
                {
                    break;
                }
                page++;
                Thread.Sleep(3000);
            }
        }
        public void ParseListpage(HttpECModel response)
        {
            var r = response.response.Content.ReadAsStringAsync();
            r.Wait();
            //Console.WriteLine(DateTime.Now.ToString()+"成功访问了百度");
            var document = parser.ParseDocument(r.Result);
            foreach(var item in document.QuerySelectorAll("div.tableWrap tr td:nth-child(1) a"))
            {
                string cName = item.TextContent;
                string url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php";
                Dictionary<string, string> _params = new Dictionary<string, string>();
                _params.Add("resource_id", "6899");
                _params.Add("query", "失信被执行人名单");
                _params.Add("cardNum", "");
                _params.Add("iname", cName);
                _params.Add("areaName", "");
                _params.Add("ie", "utf-8");
                _params.Add("oe", "utf-8");
                _params.Add("format", "json");
                _params.Add("t", DateHelper.GetTimestamp(DateTime.Now));
                _params.Add("cb", "jQuery110208514957267839198_1561021902645");
                _params.Add("_", DateHelper.GetTimestamp(DateTime.Now));
                url = url + "?" + UrlHelper.ConvertUrlParams(_params);
                var meta = new Dictionary<string, string>();
                meta.Add("companyName", cName);
                this.http.Get(url, ParseCompanyPage, meta);
            }
        }

        public void ParseCompanyPage(HttpECModel response)
        {
            Console.WriteLine((response.meta as Dictionary<string, string>)["companyName"]);
            var r = response.response.Content.ReadAsStringAsync();
            r.Wait();
            string content = r.Result;
            string jsonStr = content.Replace("/**/jQuery110208514957267839198_1561021902645(", "").Replace(");","");
            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonStr);
            foreach(var item in jsonObj?["data"]?[0]?["result"])
            {
                //自己解析
            }
        }
        public object BeforeRequest(HttpClient client,HttpRequestMessage request)
        {
            if (request.RequestUri.ToString().Contains("baidu.com"))
            {
                request.Headers.Referrer = new Uri("https://www.baidu.com/s?wd=%E5%A4%B1%E4%BF%A1%E8%A2%AB%E6%89%A7%E8%A1%8C%E4%BA%BA&rsv_spt=1&rsv_iqid=0xe03c5d860001a034&issp=1&f=3&rsv_bp=1&rsv_idx=2&ie=utf-8&rqlang=cn&tn=baiduhome_pg&rsv_enter=1&oq=%25E5%25A4%25B1%25E4%25BF%25A1%25E8%25A2%25AB%25E6%2589%25A7%25E8%25A1%258C&rsv_t=fd58%2BjDTCvnSUCJC1ekbNr77Bsd00piEZw%2BiorTCFAP304oP0zR0axx%2BmDGK44men588&rsv_pq=a0c069fa000b1335&rsv_sug3=24&rsv_sug1=21&rsv_sug7=100&rsv_sug2=1&prefixsug=%25E5%25A4%25B1%25E4%25BF%25A1%25E8%25A2%25AB%25E6%2589%25A7%25E8%25A1%258C&rsp=1&rsv_sug4=1369&rsv_sug=2");
            }
            return request;
        }
        public object BeforeResponse(HttpClient client, HttpRequestMessage request,HttpResponseMessage response)
        {
            if (request.RequestUri.ToString().Contains("11315.com"))
            {
                var r = response.Content.ReadAsStringAsync();
                r.Wait();
                string content = r.Result;
                if (content.Contains("系统检测到您的请求存在异常"))
                {
                    string capthaUrl = "http://www.11315.com/authCode.jpg?t=" + DateHelper.GetTimestamp(DateTime.Now);
                    var _task=client.GetByteArrayAsync(capthaUrl);
                    _task.Wait();
                    var result = _task.Result;

                    #region 验证码解析
                    string code = "";
                    System.IO.FileStream fs = new System.IO.FileStream("capthca.jpg", System.IO.FileMode.OpenOrCreate);
                    fs.Write(result);
                    fs.Close();
                    Console.WriteLine("验证码图片已储存，请输入验证码：");
                    code = Console.ReadLine();
                    #endregion

                    var document = parser.ParseDocument(content);
                    string url = "http://www.11315.com/validateAccess";
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("ip", document.QuerySelector("input[name='ip']").GetAttribute("value"));
                    param.Add("last_url", document.QuerySelector("input[name='last_url']").GetAttribute("value"));
                    param.Add("random", code);
                    return this.http.CreatePostRequest(url, UrlHelper.ConvertUrlParams(param), "application/x-www-form-urlencoded");
                }
            }
            return response;
        }
    }
}
