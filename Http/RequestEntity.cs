using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MyCrawler.Http
{
    public class RequestEntity
    {
        public RequestEntity(string url)
        {
            this.Request = HttpWebRequest.CreateHttp(url);
            this.Url = new Uri(url);
        }
        public RequestEntity(Uri url)
        {
            this.Request = HttpWebRequest.CreateHttp(url);
            this.Url = url;
        }
        private HttpWebRequest Request { get; set; }
        /// <summary>
        /// 请求的URL
        /// </summary>
        public Uri Url { get; set; }
        /// <summary>
        /// 请求的头信息
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 请求的COOKIE容器
        /// </summary>
        public CookieContainer Cookie { get; set; } = null;
        /// <summary>
        /// 方法
        /// </summary>
        public RequestMethod Method { get; set; } = RequestMethod.GET;
        /// <summary>
        /// Accept
        /// </summary>
        public string Accept { get; set; }
        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Connection
        /// </summary>
        public string Connection { get; set; }
        /// <summary>
        /// UserAgent
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// Referer
        /// </summary>
        public string Referer { get; set; }
        /// <summary>
        /// POST要发送的内容
        /// </summary>
        public Byte[] SendData { get; set; }
        /// <summary>
        /// 初始化一个网络访问默认实体类
        /// </summary>
        /// <param name="method"></param>
        public RequestEntity(RequestMethod method = RequestMethod.GET)
        {
            this.Headers = new Dictionary<string, string>();
            this.Method = method;
            this.Accept = "text/html, application/xhtml+xml, */*";
            this.ContentType = "application/x-www-form-urlencoded";
            SendData = new byte[0];
        }
        /// <summary>
        /// 设置POST要发送的内容 默认UTF8编码
        /// </summary>
        /// <param name="data">正文</param>
        /// <param name="encode">编码</param>
        public void SetSendData(string data, Encoding encode = null)
        {
            if (encode == null)
            {
                encode = Encoding.UTF8;
            }
            this.SendData = encode.GetBytes(data);
        }
    }
    public enum RequestMethod
    {
        POST,
        GET,
        DELETE
    }
}
