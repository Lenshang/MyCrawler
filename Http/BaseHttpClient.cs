using MyCrawler.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyCrawler.Http
{
    public abstract class BaseHttpClient
    {
        public int Id { get; set; }
        public CookieContainer CookieManager { get; set; }
        public BaseHttpClient(int clientId)
        {
            this.Id = clientId;
            this.CookieManager = new CookieContainer();
        }
        /// <summary>
        /// 根据HttpHCModel 响应请求
        /// </summary>
        /// <param name="HttpContentModel"></param>
        /// <returns></returns>
        public abstract Task<ResponseEntity> CreateHttpRequestSend(HttpContentModel model);
        /// <summary>
        /// 根据RequestEntity 响应请求
        /// </summary>
        /// <param name="requestEntity"></param>
        /// <returns></returns>
        public abstract Task<ResponseEntity> SendAsync(RequestEntity requestEntity);
    }
}
