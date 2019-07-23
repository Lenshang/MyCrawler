using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MyCrawler.Http
{
    public class ResponseEntity
    {
        /// <summary>
        /// Http响应正文
        /// </summary>
        public byte[] HtmlContent { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 响应字符集
        /// </summary>
        public string CharacterSet { get; set; }
        /// <summary>
        /// 响应头信息
        /// </summary>
        public WebHeaderCollection Headers { get; set; }
        /// <summary>
        /// 原始Response信息
        /// </summary>
        public object Response { get; set; }
        /// <summary>
        /// Response的Url信息
        /// </summary>
        public Uri ResponseUrl { get; set; }
        public string GetHtml(Encoding encode = null)
        {
            if (encode == null)
            {
                encode = Encoding.UTF8;
            }
            return encode.GetString(this.HtmlContent);
        }
        public T GetJson<T>(Encoding encode = null)
        {
            if (encode == null)
            {
                encode = Encoding.UTF8;
            }
            return JsonConvert.DeserializeObject<T>(this.GetHtml(encode));
        }
        /// <summary>
        /// JsonObject by Newtonsoft.Json
        /// </summary>
        /// <param name="encode"></param>
        /// <returns></returns>
        public JObject GetJson(Encoding encode = null)
        {
            return this.GetJson<JObject>(encode);
        }
        /// <summary>
        /// Dom Parser by AngelSharp
        /// example:document.QuerySelectorAll("")
        /// </summary>
        /// <returns>IHtmlDocument</returns>
        public IHtmlDocument GetDocument(Encoding encode = null)
        {
            var config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(this.GetHtml(encode));
            return document;
        }

        public List<string> GetAllUrl(Encoding encode = null)
        {
            var document = this.GetDocument();
            List<string> result = new List<string>();
            foreach(var item in document.QuerySelectorAll("[href]"))
            {
                string _url = item.GetAttribute("href").ToString();
                if (!_url.StartsWith("http://") && !_url.StartsWith("https://"))
                {
                    _url = new Uri(this.ResponseUrl, _url).ToString();
                }
                result.Add(_url);
            }
            return result;
        }
    }
}
