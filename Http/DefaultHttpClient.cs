using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyCrawler.Http
{
    public class DefaultHttpClient : BaseHttpClient
    {
        public DefaultHttpClient(int clientId) : base(clientId)
        {
        }
        /// <summary>
        /// 使用RequestEntity进行请求
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public override async Task<ResponseEntity> CreateHttpRequestSend(HttpContentModel model)
        {
            return await SendAsync(model.request);
        }

        public override async Task<ResponseEntity> SendAsync(RequestEntity requestEntity)
        {
            if (requestEntity.Cookie == null)
            {
                requestEntity.Cookie = this.CookieManager;
            }
            //准备
            HttpWebRequest request = HttpWebRequest.CreateHttp(requestEntity.Url);
            request.Headers = new WebHeaderCollection();
            request.ContentType = requestEntity.ContentType;
            request.Accept = requestEntity.Accept;
            //request.AllowAutoRedirect = false;
            request.Connection = requestEntity.Connection;
            request.UserAgent = requestEntity.UserAgent;
            request.Referer = requestEntity.Referer;
            foreach (var header in requestEntity.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            request.Method = requestEntity.Method.ToString();
            request.CookieContainer = requestEntity.Cookie;

            //请求
            if (requestEntity.Method == RequestMethod.POST)
            {
                request.ContentLength = requestEntity.SendData.Length;
                request.GetRequestStream().Write(requestEntity.SendData, 0, requestEntity.SendData.Length);
                request.GetRequestStream().Close();
            }
            var resultEntity = new ResponseEntity();
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch(WebException e)
            {
                response = (HttpWebResponse)e.Response;
            }
            //结果整理
            
            int readCount = 1;
            List<byte> resultHtmlByte = new List<byte>();
            byte[] buffer = new byte[1024];
            while (readCount > 0)
            {
                readCount = await response.GetResponseStream().ReadAsync(buffer, 0, buffer.Length);
                resultHtmlByte.AddRange(buffer.Take(readCount));
            }
            resultEntity.HtmlContent = resultHtmlByte.ToArray();
            resultEntity.StatusCode = (int)response.StatusCode;
            resultEntity.CharacterSet = response.CharacterSet;
            resultEntity.Headers = response.Headers;
            resultEntity.Response = response;
            resultEntity.ResponseUrl = response.ResponseUri;
            return resultEntity;
        }
    }
}
