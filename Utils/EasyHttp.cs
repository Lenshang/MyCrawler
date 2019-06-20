using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCrawler.Utils
{
    public class EasyHttp
    {
        public HttpClient client { get; set; }
        public Encoding DefaultEncode { get; set; }
        public int DefaultTimeOut { get; set; } = 1000 * 20;

        public string DefaultContentType = "application/json";
        public EasyHttp()
        {
            this.client = new HttpClient();
        }
        /// <summary>
        /// Get获得数据并自动重试
        /// </summary>
        /// <param name="url"></param>
        /// <param name="MaxRetryCount"></param>
        /// <param name="ErrorExpression"></param>
        /// <param name="SleepInterval"></param>
        /// <returns></returns>
        public string GetAndRetry(string url, int MaxRetryCount = 0, Func<string, bool> ErrorExpression = null, int SleepInterval = 1000 * 10)
        {
            //WebProxy _proxy = new WebProxy("127.0.0.1", 75);
            int retryCount = 0;
            while (retryCount < MaxRetryCount || MaxRetryCount == 0)
            {
                var resTask = this.client.GetAsync(url);
                if (!resTask.Wait(DefaultTimeOut))
                {
                    return "ERRPR==>timeout";
                }
                var res = resTask.Result;
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    string r = GetString(res.Content, DefaultEncode);
                    if (ErrorExpression != null)
                    {
                        if (ErrorExpression(r))
                        {
                            return r;
                        }
                    }
                    else
                    {
                        return r;
                    }
                }
                retryCount++;
                Thread.Sleep(SleepInterval * retryCount);
            }
            return null;
        }
        /// <summary>
        /// Post获得数据并自动重试
        /// </summary>
        /// <param name="url"></param>
        /// <param name="MaxRetryCount"></param>
        /// <param name="ErrorExpression"></param>
        /// <param name="SleepInterval"></param>
        /// <returns></returns>
        public string PostAndRetry(string url, string param, int MaxRetryCount = 0, Func<string, bool> ErrorExpression = null, int SleepInterval = 1000 * 10)
        {
            int retryCount = 0;
            while (retryCount < MaxRetryCount || MaxRetryCount == 0)
            {
                Encoding encode = Encoding.UTF8;
                if (DefaultEncode != null)
                {
                    encode = DefaultEncode;
                }
                HttpContent httpContent = new StringContent(param, encode);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(this.DefaultContentType);
                var resTask = this.client.PostAsync(url, httpContent);
                if (!resTask.Wait(DefaultTimeOut))
                {
                    return "ERRPR==>timeout";
                }
                
                var res = resTask.Result;
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    string r = GetString(res.Content, DefaultEncode);
                    if (ErrorExpression != null)
                    {
                        if (ErrorExpression(r))
                        {
                            return r;
                        }
                    }
                    else
                    {
                        return r;
                    }
                }
                retryCount++;
                Thread.Sleep(SleepInterval * retryCount);
            }
            return null;
        }
        public string GetString(HttpContent content,Encoding encode=null)
        {
            try
            {
                if (encode == null)
                {
                    encode = Encoding.UTF8;
                }
                Task<byte[]> _r = content.ReadAsByteArrayAsync();
                _r.Wait();
                byte[] r = _r.Result;
                return encode.GetString(r);
            }
            catch(Exception ex)
            {
                return "ERROR==>"+ex.Message;
            }
        }
        public string Get(string url)
        {
            var t=this.client.GetAsync(url);
            t.Wait();
            var res = t.Result;
            return GetString(res.Content, DefaultEncode);
        }
        public string Post(string url,string param,string contentType= "application/x-www-form-urlencoded")
        {
            HttpContent httpContent = new StringContent(param, DefaultEncode);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            var t = this.client.PostAsync(url, httpContent);
            t.Wait();
            var res = t.Result;
            return GetString(res.Content, DefaultEncode);
        }

        public string GetDownload(string url,string file=null)
        {
            var t = this.client.GetAsync(url);
            t.Wait();
            var res = t.Result;
            Task<byte[]> _r = res.Content.ReadAsByteArrayAsync();
            _r.Wait();
            byte[] r = _r.Result;
            if (string.IsNullOrEmpty(file))
            {
                int startPos = url.LastIndexOf("/");
                int endPos = url.LastIndexOf("?");
                file = url.Substring(startPos+1, endPos - startPos-1);
            }
            using(FileStream fs=new FileStream(file,FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                //开始写入
                bw.Write(r, 0, r.Length);
                //关闭流
                bw.Close();
            }
            return file;
        }
    }
}
